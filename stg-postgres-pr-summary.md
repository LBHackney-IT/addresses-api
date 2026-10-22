## Summary

Staging Postgres (`module.postgres_db_staging`) was still created from the unpinned external module `github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres`. Development and production already use a local `./modules/database/postgres` module.

The new local module is a copy of the external module staging was already applying. Resource addresses are unchanged, so the existing resources stay in state:

- `module.postgres_db_staging.aws_db_instance.lbh-db`
- `module.postgres_db_staging.aws_db_subnet_group.db_subnets`
- `module.postgres_db_staging.module.db_security_group.aws_security_group.lbh_db_traffic`

The instance keeps its current settings: identifier `addresses-api-db-staging`, `gp2` storage, `apply_immediately = false`, and encryption forced on. `storage_encrypted = false` on the module call was already ignored by the external module. The `instance_id` output is unchanged, so the CloudWatch alarms are unaffected.

## Change

- Point `module.postgres_db_staging` at `./modules/database/postgres`.
- Add `terraform/staging/modules/database/postgres` with the same resources, variables, and outputs as the external module. It reuses the existing staging `internal_only_traffic` security group module.
- Replace the deprecated `aws_db_instance` argument `name` with `db_name`. AWS provider 5 removed `name`. Both arguments map to the same database name (`addresses_api`), and development and production already use `db_name`.

## DMS task plan drift

Both staging DMS replication tasks showed an in-place update on every plan. AWS provider 4.67.0 removes four read-only settings when it reads a task into state, and both task settings files set them again:

- `Logging.CloudWatchLogGroup`
- `Logging.CloudWatchLogStream`
- `Logging.EnableLogContext`
- `ControlTablesSettings.historyTimeslotInMinutes`

The provider fixed this in 5.32.0.

Changes:

- Remove those four keys from `terraform/staging/task_settings.json` and `terraform/staging/task_settings_national.json`. AWS sets the CloudWatch log group and stream when `EnableLogging` is true. `HistoryTimeslotInMinutes` stays at `5`, and `EnableLogging` and the log components are unchanged.
- Load both files with `file()` instead of `templatefile()`. The task settings no longer contain template variables, so the replication-instance name and task resource ids are no longer needed.

The previous PR applied this drift. With provider 4.67.0, that update stops a running task and does not restart it, because `start_replication_task` is `false`. Check both tasks in the staging account:

- `addresses-api-es-dms-task-local-addresses` is `full-load`. Leave it stopped if its last load completed.
- `addresses-api-es-dms-task-national-addresses` is `full-load-and-cdc`. If it is stopped, resume it with `resume-processing`. Do not start it as a new full load: `TargetTablePrepMode` is `DROP_AND_CREATE`, so a fresh start drops and reloads the target.

## Plan check

The staging plan should show no changes for `module.postgres_db_staging` or either DMS replication task. Reject the plan if it shows a replacement (`-/+`) of `aws_db_instance.lbh-db`.

The module validates on AWS provider 4.67.0 (the current staging pin) and 6.52.0 (the development and production pin). A plan against the staging state was not run here because there was no active AWS SSO session for the staging account.
