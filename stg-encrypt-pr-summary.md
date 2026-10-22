## Summary

Staging Elasticsearch (`module.elasticsearch_db_staging`, domain `addresses-api-es`) now has `encrypt_at_rest = "true"`. Development and production already encrypt at rest. Staging was the remaining environment with it off.

AWS can enable encryption on the existing Elasticsearch 7.10 domain. The service runs a blue/green deployment and keeps the same domain endpoint. A new domain is not required, and encryption cannot be turned off again afterwards.

The AWS provider pin had to move first. Staging was on `provider "aws" { version = "~> 2.0" }`. In that provider, `encrypt_at_rest.enabled` is `ForceNew`, so changing `"false"` to `"true"` would destroy and recreate the domain. Provider 4.67.0 only forces a new domain when encryption is disabled, or when it is enabled on Elasticsearch older than 6.7. This domain is 7.10, so the change is an in-place update. `kms_key_id` is left unset. That argument is `ForceNew`; omitting it lets AWS use its default key and keeps the update in place.

4.67.0 is the last 4.x release, and the newest provider that still accepts `aws_db_instance.name`. Provider 5.0.0 removed that argument in favour of `db_name`. The shared Postgres module still sets `name`, so staging stays on 4.67.0 and that module does not need to change. `name` is deprecated in 4.x, so the plan will warn on it. Provider 6 would fail the plan instead.

## Change

In `terraform/staging/main.tf`:

- Pin `hashicorp/aws` to `~> 4.67.0` in `required_providers`, and remove the deprecated `version` argument from the provider block.
- Set `encrypt_at_rest` to `"true"`.
- Keep `data.aws_region.current.name`. The `region` attribute was added in provider 6.
- Replace deprecated `data.aws_subnet_ids` with `data.aws_subnets`, using the same private-subnet tag filter. `aws_subnets` exists in 4.67.0; `aws_subnet_ids` warns there and was removed in provider 5.
- Select the Elasticsearch subnet with `data.aws_subnet` on CIDR `10.120.10.64/26` (`apis-stg-private-eu-west-2a`), matching development and production. `tolist(data.aws_subnets.staging.ids)[0]` reordered the set and planned a subnet move. `vpc_options` is `ForceNew`, so that move would replace the domain.

The Elasticsearch module, instance type, node count, volume size, domain name, and subnet are unchanged.

## Plan check

The staging plan confirms an in-place update (`~`) on `module.elasticsearch_db_staging.aws_elasticsearch_domain.lbh_es`:

```text
~ encrypt_at_rest {
    ~ enabled = false -> true
  }
```

The domain is not replaced and its endpoint remains unchanged.

`vpc_options` is `ForceNew`. The domain subnet is pinned to `10.120.10.64/26`, so `subnet_ids` should be unchanged. Reject the plan if it changes.

## Unrelated DMS plan changes

The provider upgrade also produces in-place changes for both DMS replication tasks. These do not come from changes to the task settings files. Provider 4.67.0 removes the following AWS-managed/read-only values when it reads a task into Terraform state:

- `ControlTablesSettings.historyTimeslotInMinutes`
- `Logging.CloudWatchLogGroup`
- `Logging.CloudWatchLogStream`
- `Logging.EnableLogContext`

Both staging task settings files already contain those values, so Terraform plans to add them again. Provider 4.67.0 also introduces `start_replication_task` with a default of `false`, which appears as another addition because it was not present in provider 2 state.

This is a known provider 4 perpetual-diff issue that was fixed in provider 5.32.0. Applying either DMS task update can stop a running task while it is modified. Because `start_replication_task` is `false`, the provider will not restart it. This is particularly important for the national `full-load-and-cdc` task.

The encryption change can be isolated from these unrelated updates:

```shell
terraform apply -target=module.elasticsearch_db_staging.aws_elasticsearch_domain.lbh_es
```

The shared Postgres module (`github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres`) still sets `aws_db_instance.name`. On 4.67.0 that is a deprecation warning, not a plan failure.
