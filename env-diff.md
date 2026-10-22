# Terraform environment differences

Comparison of `terraform/development`, `terraform/staging`, and `terraform/production` (root `main.tf`, `alarms.tf`, local modules, and `serverless/vpc/*.yml`).

Staging is the outlier: it is pinned to an old AWS provider and an unversioned external module, while development and production use local modules on provider `~> 6.52.0`.

## 1. Provider and backend

| | development | staging | production |
|---|---|---|---|
| AWS provider | `required_providers { aws = "~> 6.52.0" }` | `provider "aws" { version = "~> 2.0" }` (deprecated inline syntax) | `required_providers { aws = "~> 6.52.0" }` |
| VPC data source | `aws_subnets` | `aws_subnet_ids` (removed in AWS provider v4+; only works on the v2 pin) | `aws_subnets` |
| Postgres/ES module source | local `./modules/database/*` | external, unpinned `github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/...` (no `?ref=`) | local `./modules/database/*` |
| State bucket | `terraform-state-development-apis` | `terraform-state-staging-apis` | `terraform-state-production-apis` |
| State key | `services/addresses-api/state` | same | same |
| Region | `eu-west-2` | `eu-west-2` | `eu-west-2` |
| VPC name tag | `apis-dev` | `apis-stg` | `apis-prod` |

This is the root cause of most drift below. Staging is stuck on an old provider pin and an external, unversioned shared module. Dev and prod have been migrated to local modules on provider v6. Resources that exist in the local modules (lambda security group, `db_port` ingress, performance insights) are not controllable in staging because they live in that external repo.

## 2. Postgres

| | development | staging | production |
|---|---|---|---|
| Engine version | 16.13 | 16.13 | 16.13 |
| Instance class | `db.t4g.small` | `db.t3.micro` | `db.t3.medium` |
| `storage_encrypted` | `true` | `false` | `true` |
| KMS key | `alias/local-backup-key` | none | `alias/local-backup-key` |
| `multi_az` | `false` | `false` | `true` |
| `storage_type` | `gp3` | module default (external) | `gp2` |
| `apply_immediately` | `true` | module default (external) | `false` |
| Performance Insights | not present | not present | enabled, 465-day retention, advanced mode |
| Port | 5501 | 5502 | 5500 |
| Allocated storage | 100 | 100 | 100 |
| Max allocated storage | 0 | not set | 0 |
| Monitoring interval | 0 | not set | 0 |
| Maintenance window | `sun:10:00-sun:10:30` | `sun:10:00-sun:10:30` | `sun:10:00-sun:10:30` |
| Backup tag | `BackupPolicy = "Dev"` | `BackupPolicy = "Prod"` | `BackupPolicy = "Prod"` |
| Deletion protection | `true` | not set | `true` |
| DB identifier | `addresses-api-db-development` | `addresses-api` | `addresses-api-db-production-emergency-temp` |
| Resource / outputs | `aws_db_instance.lbh_db` outputs `instance_endpoint`, `security_group_id` | external module; alarms use `instance_id` | `aws_db_instance.lbh-db` outputs `instance_identifier` only |
| Credentials | Terraform-managed SSM params (`/addresses-api/development/postgres-*`), value ignored after create | data sources reading existing SSM params | data sources reading existing SSM params |

`storage_encrypted = false` in staging is the only environment without encryption at rest.

Local postgres module diffs (dev vs prod):

- Resource name: `lbh_db` (dev) vs `lbh-db` (prod).
- Prod passes `db_port` into the security group module; dev does not.
- Prod `storage_type = "gp2"`; dev `storage_type = "gp3"`.
- Prod `apply_immediately = false`; dev `apply_immediately = true`.
- Prod sets Performance Insights (`enabled`, `retention_period`, `kms_key_id`, `database_insights_mode`). Dev has no `performance_insights` variable.
- Dev outputs `instance_endpoint` and `security_group_id`. Prod outputs `instance_identifier` only.

## 3. Elasticsearch

| | development | staging | production |
|---|---|---|---|
| `es_version` | 7.10 | 7.10 | 7.8 |
| `instance_type` | `t3.small.elasticsearch` | `t3.medium.elasticsearch` | `t3.medium.elasticsearch` |
| `instance_count` | 3 | 3 | 6 |
| `ebs_volume_size` | 50 | 30 | 60 |
| `ebs_enabled` | true | true | true |
| `encrypt_at_rest` | `true` | `false` | `true` |
| `zone_awareness_enabled` | false | false | false |
| Domain name | `addresses-api-es` | `addresses-api-es` | `addresses-api-es` |
| Subnet selection | explicit CIDR `10.120.6.0/25` | first private subnet from the list | explicit CIDR `10.120.8.0/26` |
| `log_publishing_options` | absent | absent (external module) | app logs and search slow logs enabled |
| `snapshot_options` | absent | absent | automated snapshot hour 23 |
| Endpoint SSM | Terraform writes `https://` + endpoint | data source reads existing SSM | data source reads existing SSM |
| Module output | `es_arn`, `es_endpoint_url`, `security_group_id` | external module | `es_arn`, `es_endpoint_url` (no `security_group_id`) |

Production is still on 7.8. Production log groups are hardcoded to account `153306643385`:

- `arn:aws:logs:eu-west-2:153306643385:log-group:/aws/aes/domains/addresses-api-es/application-logs`
- `arn:aws:logs:eu-west-2:153306643385:log-group:/aws/aes/domains/addresses-api-es/search-logs`

## 4. Security groups

Development builds a dedicated `aws_security_group.lambda` (`addresses-api-lambda-development`) and explicit `aws_security_group_rule`s scoping Postgres and Elasticsearch ingress to specific source security groups (bastion, lambda, DMS). The lambda SG id is published to `/addresses-api/development/lambda-security-group-id` and consumed by `serverless/vpc/development.yml`.

Production and staging have no equivalent resources in this repo.

Dev bastion SG (hardcoded): `sg-073fee129434a7e0c`.

`serverless/vpc` security groups and subnets:

| | development | staging | production |
|---|---|---|---|
| Security groups | SSM `/addresses-api/development/lambda-security-group-id` | hardcoded `sg-0721d1d066b247b8f`, `sg-0828e84a9e427392a` | hardcoded `sg-0559e24a4d1ee0ddc`, `sg-0d6c0357cc2febe32` |
| Subnets | `subnet-0deabb5d8fb9c3446`, `subnet-000b89c249f12a8ad` | `subnet-06d3de1bd9181b0d7`, `subnet-0ed7d7713d1127656` | `subnet-01d3657f97a243261`, `subnet-0b7b8fea07efabf34` |

Shared `internal_only_traffic` module (dev vs prod):

- Prod has an ingress rule open to `0.0.0.0/0` on `var.db_port`, with `lifecycle { ignore_changes = [ingress] }`.
- Dev has egress only. Ingress is added separately as scoped `aws_security_group_rule`s.
- Prod requires a `db_port` variable; dev does not.

Staging lambda security groups are raw IDs in `serverless/vpc/staging.yml`, unmanaged by this repo's Terraform.

## 5. DMS

| | development | staging | production |
|---|---|---|---|
| Replication instance | local `./modules/dms` | local `./modules/dms` | not managed; hardcoded `replication_instance_arn` |
| Instance class | `dms.t3.small` | `dms.t3.small` | n/a (`production-dms-instance` referenced in task settings only) |
| Engine version | `3.6.1` | `3.5.4` | n/a |
| Maintenance window | `sun:10:00-sun:10:30` | `Sun:08:00-Sun:08:30` | n/a |
| VPC security groups | `var.vpc_security_group_ids` from `module.dms_security_group` | hardcoded `sg-0f2b37cc9a9e2cb60` (ignores the variable) | n/a |
| `dms-vpc-role` | managed | managed | absent |
| `dms-cloudwatch-logs-role` | managed | absent | absent |
| ES policy name | `DMS_Elasticsearch_Addresses` | `DMS_Elasticsearch_Addresses_API` | `DMS_Elasticsearch_Addresses` |
| Source endpoint `ssl_mode` | `require` | `none` | `none` |
| ES target `ssl_mode` | `none` | `none` | `none` |
| ES target URI | module endpoint hostname | SSM parameter value | SSM parameter value |
| Local addresses task | `full-load`, `file()` task settings, env `development` | `full-load`, `templatefile()` with hardcoded task resource `ZMFZ7R2ZF5BGFNU6T3OTYNWJPE`, env `stg` | `full-load`, `templatefile()` with resource `LM6NMGMJLYKDTL7SIE3PXS6RZIYDVGDIC2RL3ZI`, hardcoded instance ARN `...rep:65CJ5HE2DMCUW5X6EPKTKUDVWA` |
| National addresses task | `full-load`, `file()` task settings | `full-load-and-cdc`, `templatefile()` of `task_settings_national.json`, resource `ICYNOPE3CNGK7FCOIEUT5XCQ2A` | absent |
| Premigration assessment (S3, KMS, IAM, SSM) | `premigration_assessment.tf` | absent | absent |

Staging's DMS module ignores `var.vpc_security_group_ids`. Production's replication instance is not in Terraform.

## 6. CloudWatch alarms

Only staging and production have `alarms.tf`. Development has none.

Both alarm on `Platform-APIs-Alerts`:

- `address-api-db-transactions-log-disk-usage-too-high` — `TransactionLogsDiskUsage` greater than 40000 MB.
- `address-api-db-free-storage-space-too-low` — `FreeStorageSpace` less than 40000 MB.

Dimension differs because of the postgres module outputs:

- Staging: `module.postgres_db_staging.instance_id`
- Production: `module.postgres_db_production.instance_identifier`

## 7. SSM parameters written by Terraform

Development creates:

- `/addresses-api/development/postgres-password` (SecureString, ignore changes)
- `/addresses-api/development/postgres-username` (SecureString, ignore changes)
- `/addresses-api/development/postgres-port`
- `/addresses-api/development/postgres-hostname`
- `/addresses-api/development/lambda-security-group-id`
- `/addresses-api/development/elasticsearch-domain` (`https://` + endpoint)
- `/addresses-api/development/reindexing-queue`
- `/addresses-api/development/dms-rep-instance-arn`
- `/addresses-api/development/dms-premigration-assessment-bucket`
- `/addresses-api/development/dms-premigration-assessment-role-arn`
- `/addresses-api/development/dms-premigration-assessment-kms-key-arn`

Staging creates only `/addresses-api/staging/dms-rep-instance-arn`. Postgres, ES domain, and other values are read as data sources.

Production creates none of these. Postgres hostname/username/password and the ES domain are data sources. The DMS instance ARN is a literal.

`serverless.yml` expects these SSM paths in every stage: `postgres-hostname`, `postgres-port`, `postgres-username`, `postgres-password`, `elasticsearch-domain`, `reindexing-queue`.

## 8. Serverless stage differences (not Terraform, but coupled)

| | development | staging | production |
|---|---|---|---|
| API timeout | 15 | 15 | 45 |
| Nightly reindex cron | disabled | enabled | enabled |
| Authorizer account | `859159924354` | `715003523189` | `153306643385` |

## Sync priorities

1. Production Elasticsearch to 7.10. Consider porting production `log_publishing_options` and `snapshot_options` to dev and staging.
2. Staging provider pin from `~> 2.0` to `~> 6.52.0`, and move staging off the unpinned external module onto the local `./modules/database/*` used by dev and production.
3. Enable `storage_encrypted` and `encrypt_at_rest` in staging.
4. Replace production's `0.0.0.0/0` DB ingress with development's scoped security group rules. Bring a Terraform-managed lambda security group to staging (currently hardcoded IDs in `serverless/vpc/staging.yml`).
5. Fix staging DMS so it uses `var.vpc_security_group_ids`. Decide whether production's DMS instance should be managed like dev and staging.
6. Align postgres resource names (`lbh_db` vs `lbh-db`) and module outputs (`instance_endpoint` / `security_group_id` vs `instance_identifier`) so all three environments expose the same outputs.
