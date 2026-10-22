## Summary

Staging Elasticsearch was still created from the unpinned external module `github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/elasticsearch`. Development and production already use a local `./modules/database/elasticsearch` module.

Switching the source does not change Terraform resource addresses, so the existing domain, service-linked role, and security group stay in state:

- `module.elasticsearch_db_staging.aws_elasticsearch_domain.lbh_es`
- `module.elasticsearch_db_staging.aws_iam_service_linked_role.es`
- `module.elasticsearch_db_staging.module.db_security_group.aws_security_group.lbh_db_traffic`

The local module keeps the settings already applied in staging: Elasticsearch 7.10, `t3.small.elasticsearch`, 3 nodes, 50 GB, encryption at rest off, and the first private subnet. Automated snapshots stay at hour 23. The security group matches development: egress only, with no inline ingress and no `ignore_changes` on ingress.

`zone_awareness_config` is dynamic, matching development and production. Zone awareness is off, so the block is omitted. AWS provider 2.70 suppresses that omission when the live domain has no zone-awareness config.

Production log publishing was not copied. Those log groups are hardcoded to the production account and would plan new options on the staging domain.

Postgres stays on the external module, and the AWS provider pin stays at `~> 2.0`.

## Change

- Point `module.elasticsearch_db_staging` at `./modules/database/elasticsearch`.
- Add `terraform/staging/modules/database/elasticsearch` with the same resources and outputs as the external module (`es_arn`, `es_endpoint_url`).
- Add `terraform/staging/modules/security_groups/database/internal_only_traffic`, which that module calls for the domain security group.

The next staging plan should show the Elasticsearch security group losing its inline ingress rule. The domain itself should be unchanged. Staging does not yet have the scoped Lambda, bastion, and DMS ingress rules that development adds outside this module.

The module validates against AWS provider 2.70.0. A plan against the staging state was not run here: local AWS credentials were rejected, and Terraform could not download the remaining private GitHub modules (Postgres and DMS).
