# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

terraform {
  backend "s3" {
    bucket  = "terraform-state-production-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/addresses-api/state"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.52.0"
    }
  }
}

provider "aws" {
  region = "eu-west-2"
}

data "aws_region" "current" {}
data "aws_caller_identity" "current" {}

locals {
  current_aws_region              = data.aws_region.current.region
  parameter_store                 = "arn:aws:ssm:${local.current_aws_region}:${data.aws_caller_identity.current.account_id}:parameter"
  dms_replication_subnet_group_id = "dms-replication-subnet-group-production-dms-instance"
}

/*    VPC SET UP    */

data "aws_vpc" "production_vpc" {
  tags = {
    Name = "apis-prod"
  }
}

data "aws_subnets" "production" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.production_vpc.id]
  }

  tags = {
    Type = "private"
  }
}

/*    POSTGRES SET UP    */

data "aws_ssm_parameter" "addresses_postgres_db_password" {
  name = "/addresses-api/production/postgres-password"
}

data "aws_ssm_parameter" "addresses_postgres_username" {
  name = "/addresses-api/production/postgres-username"
}

data "aws_ssm_parameter" "addresses_postgres_hostname" {
  name = "/addresses-api/production/postgres-hostname"
}

data "aws_kms_key" "local_backup_key" {
  key_id = "alias/local-backup-key"
}

module "postgres_db_production" {
  source                   = "./modules/database/postgres"
  environment_name         = "production"
  vpc_id                   = data.aws_vpc.production_vpc.id
  db_identifier            = "addresses-api-db-production-emergency-temp"
  db_name                  = "addresses_api"
  db_port                  = 5500
  subnet_ids               = data.aws_subnets.production.ids
  db_engine                = "postgres"
  db_engine_version        = "16.13"
  db_instance_class        = "db.t3.medium"
  db_allocated_storage     = 100
  db_max_allocated_storage = 0
  monitoring_interval      = 0
  maintenance_window       = "sun:10:00-sun:10:30"
  db_username              = data.aws_ssm_parameter.addresses_postgres_username.value
  db_password              = data.aws_ssm_parameter.addresses_postgres_db_password.value
  storage_encrypted        = true
  kms_key_id               = data.aws_kms_key.local_backup_key.arn
  multi_az                 = true //only true if production deployment
  publicly_accessible      = false
  project_name             = "platform apis"
  deletion_protection      = true
  copy_tags_to_snapshot    = true
  performance_insights = {
    enabled = true
    # Based on AWS docs: "To turn on Database Insights, the retention period must be at least 465 days."
    retention_period = 465
    kms_key_id       = data.aws_kms_key.local_backup_key.arn
    mode             = "advanced"
  }
  additional_tags = {
    BackupPolicy = "Prod"
  }
}

/*    ELASTICSEARCH SETUP    */

# When switching from `aws_subnet_ids` to `aws_subnets` data blocks the order of subnets has changed, 
# and TF tries to move the ES domain a different subnet. To prevent this, the previously used subnet
# was filtered by CIDR that is the definition of the subnet id used by this ES domain (see definitions):
# https://github.com/LBHackney-IT/infrastructure/blob/979206edd3539b11fb17e00c3d97ca849fb713ed/projects/apis-production/config/terraform/prod.tfvars#L3
data "aws_subnet" "addreses-es-domain" {
  vpc_id     = data.aws_vpc.production_vpc.id
  cidr_block = "10.120.8.0/26"
}

module "elasticsearch_db_production" {
  source           = "./modules/database/elasticsearch"
  vpc_id           = data.aws_vpc.production_vpc.id
  environment_name = "production"
  port             = 443
  domain_name      = "addresses-api-es"
  subnet_ids       = [data.aws_subnet.addreses-es-domain.id]
  project_name     = "addresses-api"
  es_version       = "7.10"
  encrypt_at_rest  = "true"
  instance_type    = "t3.medium.elasticsearch"
  instance_count   = "6"
  ebs_enabled      = "true"
  ebs_volume_size  = "60"
  region           = local.current_aws_region
  account_id       = data.aws_caller_identity.current.account_id

  zone_awareness_enabled = false
}

data "aws_ssm_parameter" "addresses_elasticsearch_domain" {
  name = "/addresses-api/production/elasticsearch-domain"
}

/*    DMS SETUP    */
data "aws_iam_policy_document" "dms-assume-role-policy" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["dms.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "dms_service_role" {
  name               = "dms_service_role"
  path               = "/system/"
  assume_role_policy = data.aws_iam_policy_document.dms-assume-role-policy.json
}

resource "aws_iam_policy" "es_policy" {
  name        = "DMS_Elasticsearch_Addresses"
  description = "A policy allowing you CRUD operations on addresses API elasticsearch cluster"

  policy = <<EOF
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                       "es:ESHttpDelete",
                       "es:ESHttpGet",
                       "es:ESHttpHead",
                       "es:ESHttpPost",
                       "es:ESHttpPut"
                     ],
            "Resource": "${module.elasticsearch_db_production.es_arn}"
        }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "attach_policy" {
  role       = aws_iam_role.dms_service_role.name
  policy_arn = aws_iam_policy.es_policy.arn
}

# Existing instance created outside this state. Subnet group id matches the
# development module naming (dms-replication-subnet-group-<instance id>).
import {
  to = module.dms_replication_instance_production.aws_dms_replication_subnet_group.dms_subnet_group
  id = local.dms_replication_subnet_group_id
}

import {
  to = module.dms_replication_instance_production.aws_dms_replication_instance.address_dms_rep_instance
  id = "production-dms-instance"
}

module "dms_security_group" {
  source           = "./modules/security_groups/dms"
  vpc_id           = data.aws_vpc.production_vpc.id
  environment_name = "production"
}

resource "aws_security_group_rule" "postgres_dms_ingress" {
  type                     = "ingress"
  description              = "allow inbound traffic from DMS replication instance"
  from_port                = 5500
  to_port                  = 5500
  protocol                 = "tcp"
  security_group_id        = module.postgres_db_production.security_group_id
  source_security_group_id = module.dms_security_group.dms_sg_id
}

resource "aws_security_group_rule" "elasticsearch_dms_ingress" {
  type                     = "ingress"
  description              = "allow inbound traffic from DMS replication instance"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = module.elasticsearch_db_production.security_group_id
  source_security_group_id = module.dms_security_group.dms_sg_id
}

module "dms_replication_instance_production" {
  source                          = "./modules/dms"
  environment_name                = "production"
  project_name                    = "addresses-api"
  replication_instance_identifier = "production-dms-instance"
  replication_instance_class      = "dms.t3.small"
  replication_subnet_group_id     = local.dms_replication_subnet_group_id
  engine_version                  = "3.6.1"
  allocated_storage               = 20
  availability_zone               = "eu-west-2a"
  auto_minor_version_upgrade      = true
  multi_az                        = false
  vpc_id                          = data.aws_vpc.production_vpc.id
  subnet_ids                      = data.aws_subnets.production.ids
  maintenance_window              = "sun:10:00-sun:10:30"
  vpc_security_group_ids          = [module.dms_security_group.dms_sg_id]
}

resource "aws_ssm_parameter" "dms_rep_instance_arn" {
  name  = "/addresses-api/production/dms-rep-instance-arn"
  type  = "String"
  value = module.dms_replication_instance_production.dms_rep_instance_arn
}

resource "aws_dms_endpoint" "address_elasticsearch" {
  endpoint_id   = "target-addresses-es"
  endpoint_type = "target"
  engine_name   = "elasticsearch"
  port          = 443
  ssl_mode      = "none"

  elasticsearch_settings {
    endpoint_uri            = data.aws_ssm_parameter.addresses_elasticsearch_domain.value
    service_access_role_arn = aws_iam_role.dms_service_role.arn
  }

  tags = {
    Name         = "target-addresses-es",
    Environment  = "production",
    project_name = "addresses-api"
  }
}

module "source_db_endpoint" {
  source                  = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_endpoint"
  database_name           = "addresses_api"
  dms_endpoint_identifier = "source-addresses-postgres"
  endpoint_type           = "source"
  engine_name             = "postgres"
  database_port           = 5500
  db_server               = data.aws_ssm_parameter.addresses_postgres_hostname.value
  ssl_mode                = "none"
  environment_name        = "production"
  project_name            = "addresses-api"
  db_username             = data.aws_ssm_parameter.addresses_postgres_username.value
  db_password             = data.aws_ssm_parameter.addresses_postgres_db_password.value
}

module "address-es-dms-local-addresses" {
  source                       = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_replication_task"
  environment_name             = "production"
  project_name                 = "addresses-api"
  migration_type               = "full-load"
  replication_instance_arn     = aws_ssm_parameter.dms_rep_instance_arn.value
  replication_task_indentifier = "addresses-api-es-dms-task-local-addresses"
  task_settings = templatefile("${path.module}/task_settings.json",
    {
      dms_replication_instance_name = "production-dms-instance",
      dms_instance_task_resource    = "LM6NMGMJLYKDTL7SIE3PXS6RZIYDVGDIC2RL3ZI"
    }
  )
  source_endpoint_arn = module.source_db_endpoint.dms_endpoint_arn
  target_endpoint_arn = aws_dms_endpoint.address_elasticsearch.endpoint_arn
  task_table_mappings = file("${path.module}/selection_rules_local.json")
}
