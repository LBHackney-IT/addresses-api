# PR summary: Addresses API development environment (SSI-1182)

## Summary

Adds a full **development** data platform and Serverless wiring for Addresses API: PostgreSQL (RDS), Elasticsearch, DMS full-load replication into ES, Lambda networking, SSM parameters, premigration assessment support, and docs. Nightly reindex is **disabled** in development; reindex and DMS tasks are run manually.

## What this PR delivers

### Terraform (`terraform/development/`)

- **Postgres** — `addresses-api-db-development` (Postgres 16.13, `db.t4g.small`, port **5501**, encrypted, single-AZ), restored/imported into Terraform; credentials and connection details in SSM.
- **Elasticsearch** — domain `addresses-api-es` (ES **7.10**, currently `t3.medium` × **3**, 30 GB EBS, encrypt at rest) for DMS load and searchable reindexed aliases.
- **DMS** — replication instance `development-dms-instance` (`dms.t3.small`, engine **3.6.1**), source/target endpoints, and two **manual `full-load`** tasks (Hackney + national).
- **Security groups** — dedicated Lambda SG; explicit bastion / Lambda / DMS ingress to Postgres (5501) and Elasticsearch (443).
- **SSM** — parameters under `/addresses-api/development/*` (Postgres, ES URL with `https://`, reindex queue URL, Lambda SG id, DMS/premigration ARNs).
- **Premigration assessment** — S3, KMS, IAM, and SSM (`premigration_assessment.tf`).
- **Docs** — `terraform/development/README.md` (architecture, SG paths, DMS flow, operational notes).

### Serverless (`serverless.yml`)

- Development VPC uses Lambda SG from SSM (`/addresses-api/development/lambda-security-group-id`).
- Hackney nightly reindex cron **off** in development (`custom.reindexScheduleEnabled.development: false`); comments document manual Lambda Test payloads.
- National reindex cron remains disabled in all stages (unchanged).

### CI

- CircleCI development Terraform and application deploy paths are available for this environment.

## How the stack works (development)

1. **Postgres** holds `hackney_address` / `national_address` (dev DB is a copy of production data).
2. **DMS full-load** (manual start) copies those tables into ES indices `hackney_address` / `national_address`.
3. **Reindex Lambda** (manual) copies DMS indices into dated indices (e.g. `hackney_addresses_YYYYMMDD…`) using `data/elasticsearch/index.json` mappings.
4. **Switch-alias Lambda** (SQS-triggered, ~10 minute message delay) attaches aliases `hackney_addresses` / `national_addresses`.
5. **API** searches those **aliases** only — not the raw DMS indices.

## Differences from staging and production

### High-level

| Area | Development | Staging | Production |
|------|-------------|---------|------------|
| Intent | Bring-up / manual ops | Always-on lower env | Live HA |
| Terraform completeness | Self-contained (modules, SGs, SSM, DMS instance, premigration) | Older pattern (remote common modules, hardcoded DMS SG) | Large Postgres/ES; DMS instance mostly outside TF |
| Reindex schedule | **Disabled** (manual) | Hackney cron **on** | Hackney cron **on** |
| DMS national task | `full-load` (manual) | `full-load-and-cdc` | Not defined in TF `main.tf` (local task only) |

### Postgres

| | Development | Staging | Production |
|---|-------------|---------|------------|
| Identifier | `addresses-api-db-development` | `addresses-api` | `addresses-api-db-production-emergency-temp` |
| Port | **5501** | **5502** | **5500** |
| Class | `db.t4g.small` | `db.t3.micro` | `db.t3.medium` |
| Multi-AZ | No | No | **Yes** |
| Encryption | Yes (KMS) | No | Yes (KMS) |
| Module source | Local `./modules/database/postgres` | Remote common TF module | Local module |
| SSM | **Creates** params | Reads existing | Reads existing |

### Elasticsearch

| | Development | Staging | Production |
|---|-------------|---------|------------|
| Version | **7.10** | 7.8 | 7.8 |
| Size | `t3.medium` × **3** (sized for full load; can scale down when idle) | `t3.medium` × 3 | `t3.medium` × **6**, 60 GB EBS |
| Encrypt at rest | Yes | No | Yes |
| Extra | — | — | App + search slow logs; snapshots |

### DMS

| | Development | Staging | Production |
|---|-------------|---------|------------|
| Instance in TF | Yes (`development-dms-instance`) | Yes (`staging-dms-instance`) | **No** — ARN hardcoded |
| Engine | **3.6.1** | 3.5.4 | External |
| Source SSL | **`require`** (matches `rds.force_ssl`) | `none` | `none` |
| Local task | `full-load` | `full-load` | `full-load` |
| National task | `full-load` | **`full-load-and-cdc`** | Missing from TF |
| Premigration stack | **Yes** | No | No |
| Extra IAM | Includes `dms-cloudwatch-logs-role` | VPC + service role | Service role + ES policy |

### Networking / Lambdas

| | Development | Staging | Production |
|---|-------------|---------|------------|
| Lambda SG | Terraform-managed + SSM | Hardcoded in `serverless.yml` | Hardcoded in `serverless.yml` |
| Bastion → PG / ES | Explicit TF rules (SG `sg-073fee129434a7e0c`) | Not in TF | Not in TF |
| DMS → PG / ES | Explicit TF rules | Pre-existing SG | N/A in TF |

### Serverless schedules

| Schedule | Development | Staging | Production |
|----------|-------------|---------|------------|
| Hackney reindex `cron(0 01 * * ? *)` | **Off** | On | On |
| National reindex `cron(0 02 * * ? *)` | Off | Off | Off |

## Intentional development deviations (why they differ)

1. **No nightly reindex** — avoids surprise cost/load while standing up the env; use Lambda console Test (see `serverless.yml` comments).
2. **Manual DMS full-load only** — both tasks; no CDC on national (unlike staging).
3. **Explicit SG model** — Lambda / bastion / DMS ingress in Terraform for clearer least-privilege and easier debugging (e.g. bastion `curl` to ES).
4. **DMS `ssl_mode = require`** — required for encrypted RDS connections in development.
5. **ES 7.10** — current AWS Elasticsearch service version used for this domain (staging/prod remain on 7.8).
6. **Premigration assessment resources** — support running DMS assessments in development.
7. **Creates (not only consumes) SSM** — greenfield parameter ownership for the new account/env.

## Operational checklist (after deploy)

1. Apply Terraform (development), then deploy Serverless so Lambdas pick up SG + SSM.
2. Start DMS full-load tasks when ready; confirm indices `hackney_address` / `national_address`.
3. Manually invoke reindex Lambdas for `hackney_addresses` and `national_addresses` (with `fromIndex` = DMS index names).
4. Wait for SQS delay (~10 minutes) and switch-alias; confirm `_cat/aliases` shows both aliases.
5. API search hits **aliases** only — e.g. `/api/v2/addresses?postcode=E8` needs `hackney_addresses` attached.

## Further reading

- [terraform/development/README.md](terraform/development/README.md)
- [docs/elasticsearch_setup.md](docs/elasticsearch_setup.md)
- [docs/manual_reindexing.md](docs/manual_reindexing.md)
