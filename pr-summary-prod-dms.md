## Summary

Production DMS was the remaining environment without a managed replication instance. The local-addresses task used a hardcoded ARN (`rep:65CJ5HE2DMCUW5X6EPKTKUDVWA`). Development and staging already manage the instance with a local `./modules/dms` module.

This adds `terraform/production/modules/dms` and import blocks for the existing instance and subnet group. Settings match what is deployed, so the apply imports them into state and does not modify them.

## Change

- Add `terraform/production/modules/dms` and call it as `module.dms_replication_instance_production`.
- Import `production-dms-instance` and `dms-replication-subnet-group-production-dms-instance`.
- Point the local-addresses task at `module.dms_replication_instance_production.dms_rep_instance_arn` instead of the hardcoded ARN.

Live settings kept as-is:

- Engine `3.5.4`, class `dms.t3.small`, 20 GB, `eu-west-2a`, maintenance window `sun:07:00-sun:07:30`.
- Tag and subnet-group description `project_name = resident-information-apis`.
- Subnets `subnet-01d3657f97a243261` and `subnet-0b7b8fea07efabf34`.
- Security group `sg-053a1e67a48b36147`.

`prevent_destroy` is set on the instance. `availability_zone`, `publicly_accessible`, and the subnet group id force replacement, so a plan that replaces the instance should be rejected.

No new security group, database ingress rules, or SSM parameter. Those do not exist in production. An SSM value is also sensitive and made the task look changed even though the ARN was the same.

## Plan check

After matching the live values, the production plan should be:

```text
Plan: 2 to import, 0 to add, 0 to change, 0 to destroy.
```

Apply only writes the two resources into state. It does not change the instance, subnet group, or replication task.
