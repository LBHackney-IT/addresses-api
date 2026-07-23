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
    bucket  = "terraform-state-development-apis"
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

data "aws_caller_identity" "current" {}
data "aws_region" "current" {}
locals {
  parameter_store    = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
  db_port            = 5501
  current_aws_region = data.aws_region.current.region
}

/*    VPC SET UP    */
data "aws_vpc" "development_vpc" {
  tags = {
    Name = "apis-dev"
  }
}

data "aws_subnets" "development" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.development_vpc.id]
  }

  tags = {
    Type = "private"
  }
}

data "aws_kms_key" "local_backup_key" {
  key_id = "alias/local-backup-key"
}

# // db restored from backup and imported to terraform
import {
  id = "addresses-api-db-development"
  to = module.postgres_db_development.aws_db_instance.lbh_db
}

//resource deployed first separately and then used in the db module after value changes
resource "aws_ssm_parameter" "addresses_postgres_db_password" {
  description = "Addresses API Development Postgres DB Password"
  name        = "/addresses-api/development/postgres-password"
  type        = "SecureString"
  value       = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

//resource deployed first separately and then used in the db module after value changes
resource "aws_ssm_parameter" "addresses_postgres_db_username" {
  description = "Addresses API Development Postgres DB Username"
  name        = "/addresses-api/development/postgres-username"
  type        = "SecureString"
  value       = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "addresses_postgres_db_port" {
  description = "Addresses API Development Postgres DB Port"
  name        = "/addresses-api/development/postgres-port"
  type        = "String"
  value       = local.db_port
}

resource "aws_ssm_parameter" "addresses_postgres_db_hostname" {
  description = "Addresses API Development Postgres DB Hostname"
  name        = "/addresses-api/development/postgres-hostname"
  type        = "String"
  value       = module.postgres_db_development.instance_endpoint
}

module "postgres_db_development" {
  source                   = "./modules/database/postgres"
  environment_name         = "development"
  vpc_id                   = data.aws_vpc.development_vpc.id
  db_identifier            = "addresses-api-db-development"
  db_name                  = "addresses_api"
  db_port                  = local.db_port
  subnet_ids               = data.aws_subnets.development.ids
  db_engine                = "postgres"
  db_engine_version        = "16.13"
  db_instance_class        = "db.t4g.small"
  db_allocated_storage     = 100
  db_max_allocated_storage = 0
  monitoring_interval      = 0
  maintenance_window       = "sun:10:00-sun:10:30"
  db_username              = aws_ssm_parameter.addresses_postgres_db_username.value
  db_password              = aws_ssm_parameter.addresses_postgres_db_password.value
  storage_encrypted        = true
  kms_key_id               = data.aws_kms_key.local_backup_key.arn
  multi_az                 = false
  publicly_accessible      = false
  project_name             = "platform apis"
  deletion_protection      = true
  copy_tags_to_snapshot    = true
  additional_tags = {
    BackupPolicy = "Dev"
  }
}

# Bastion access is postgres-specific; keep it outside the shared DB security group module
# so elasticsearch (and other consumers) are not forced to accept the same ingress.
resource "aws_security_group_rule" "postgres_bastion_ingress" {
  type                     = "ingress"
  description              = "allow inbound traffic from bastion host"
  from_port                = local.db_port
  to_port                  = local.db_port
  protocol                 = "tcp"
  security_group_id        = module.postgres_db_development.security_group_id
  source_security_group_id = "sg-073fee129434a7e0c"
}

/*    ELASTICSEARCH SETUP    */

#apis-dev-private-eu-west-2b
data "aws_subnet" "addreses-es-domain" {
  vpc_id     = data.aws_vpc.development_vpc.id
  cidr_block = "10.120.6.0/25"
}

// robust one-off load/re-load config is 3 instances with t3.medium.elasticsearch
// this has been scaled back to a minimum since we don't have any scheduled tasks on development
// please see the Serverless.yml for details if you want to trigger a one-off re-run
module "elasticsearch_db_development" {
  source           = "./modules/database/elasticsearch"
  vpc_id           = data.aws_vpc.development_vpc.id
  environment_name = "development"
  port             = 443
  domain_name      = "addresses-api-es"
  subnet_ids       = [data.aws_subnet.addreses-es-domain.id]
  project_name     = "addresses-api"
  es_version       = "7.10"
  encrypt_at_rest  = "true"
  instance_type    = "t3.medium.elasticsearch"
  instance_count   = "3"
  ebs_enabled      = "true"
  ebs_volume_size  = "30"
  region           = data.aws_region.current.name
  account_id       = data.aws_caller_identity.current.account_id

  zone_awareness_enabled = false
}

resource "aws_ssm_parameter" "addresses_elasticsearch_domain" {
  description = "Addresses API Development Elasticsearch Domain"
  name        = "/addresses-api/development/elasticsearch-domain"
  type        = "String"
  # aws_elasticsearch_domain.endpoint is hostname-only; the app expects a full URI (see Startup.ConfigureElasticsearch)
  value = "https://${module.elasticsearch_db_development.es_endpoint_url}"
}

# Queue URL for the reindex Lambdas (SQS_QUEUE_URL in serverless.yml).
# The queue itself is created by Serverless CloudFormation as sqsQueueReindexingAlias
# with QueueName sqs-addresses-api-reindex-${stage} — see serverless.yml.
resource "aws_ssm_parameter" "reindexing_queue" {
  description = "Addresses API Development reindex SQS queue URL"
  name        = "/addresses-api/development/reindexing-queue"
  type        = "String"
  value       = "https://sqs.${data.aws_region.current.name}.amazonaws.com/${data.aws_caller_identity.current.account_id}/sqs-addresses-api-reindex-development"
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

# Account-level role required for DMS to manage ENIs in the VPC
resource "aws_iam_role" "dms-vpc-role" {
  name               = "dms-vpc-role"
  assume_role_policy = data.aws_iam_policy_document.dms-assume-role-policy.json
}

resource "aws_iam_role_policy_attachment" "dms-vpc-role-AmazonDMSVPCManagementRole" {
  role       = aws_iam_role.dms-vpc-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonDMSVPCManagementRole"
}

# Account-level role required for DMS task CloudWatch logging
# (role name is case-sensitive and must be exactly dms-cloudwatch-logs-role)
resource "aws_iam_role" "dms-cloudwatch-logs-role" {
  description        = "Role for DMS task CloudWatch logging. Created by Addresses API Terraform."
  name               = "dms-cloudwatch-logs-role"
  assume_role_policy = data.aws_iam_policy_document.dms-assume-role-policy.json
  tags = {
    Name         = "dms-cloudwatch-logs-role",
    Environment  = "development",
    project_name = "addresses-api"
  }
}

resource "aws_iam_role_policy_attachment" "dms-cloudwatch-logs-role-AmazonDMSCloudWatchLogsRole" {
  role       = aws_iam_role.dms-cloudwatch-logs-role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonDMSCloudWatchLogsRole"
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
            "Resource": "${module.elasticsearch_db_development.es_arn}"
        }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "attach_policy" {
  role       = aws_iam_role.dms_service_role.name
  policy_arn = aws_iam_policy.es_policy.arn
}

module "dms_security_group" {
  source           = "./modules/security_groups/dms"
  vpc_id           = data.aws_vpc.development_vpc.id
  environment_name = "development"
}

resource "aws_security_group_rule" "postgres_dms_ingress" {
  type                     = "ingress"
  description              = "allow inbound traffic from DMS replication instance"
  from_port                = local.db_port
  to_port                  = local.db_port
  protocol                 = "tcp"
  security_group_id        = module.postgres_db_development.security_group_id
  source_security_group_id = module.dms_security_group.dms_sg_id
}

resource "aws_security_group_rule" "elasticsearch_dms_ingress" {
  type                     = "ingress"
  description              = "allow inbound traffic from DMS replication instance"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = module.elasticsearch_db_development.security_group_id
  source_security_group_id = module.dms_security_group.dms_sg_id
}

module "dms_replication_instance_development" {
  source                          = "./modules/dms"
  environment_name                = "development"
  project_name                    = "addresses-api"
  replication_instance_identifier = "development-dms-instance"
  replication_instance_class      = "dms.t3.small"
  vpc_id                          = data.aws_vpc.development_vpc.id
  subnet_ids                      = data.aws_subnets.development.ids
  maintenance_window              = "sun:10:00-sun:10:30"
  vpc_security_group_ids          = [module.dms_security_group.dms_sg_id]

  depends_on = [
    aws_iam_role_policy_attachment.dms-vpc-role-AmazonDMSVPCManagementRole,
    aws_iam_role_policy_attachment.dms-cloudwatch-logs-role-AmazonDMSCloudWatchLogsRole,
  ]
}

resource "aws_ssm_parameter" "dms_rep_instance_arn" {
  name  = "/addresses-api/development/dms-rep-instance-arn"
  type  = "String"
  value = module.dms_replication_instance_development.dms_rep_instance_arn
}

resource "aws_dms_endpoint" "address_elasticsearch" {
  endpoint_id   = "target-addresses-es"
  endpoint_type = "target"
  engine_name   = "elasticsearch"
  port          = 443
  ssl_mode      = "none"

  elasticsearch_settings {
    endpoint_uri            = aws_ssm_parameter.addresses_elasticsearch_domain.value
    service_access_role_arn = aws_iam_role.dms_service_role.arn
  }

  tags = {
    Name         = "target-addresses-es",
    Environment  = "development",
    project_name = "addresses-api"
  }
}

module "source_db_endpoint" {
  source                  = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_endpoint"
  database_name           = "addresses_api"
  dms_endpoint_identifier = "source-addresses-postgres"
  endpoint_type           = "source"
  engine_name             = "postgres"
  database_port           = local.db_port
  db_server               = aws_ssm_parameter.addresses_postgres_db_hostname.value
  ssl_mode                = "require"
  environment_name        = "development"
  project_name            = "addresses-api"
  db_username             = aws_ssm_parameter.addresses_postgres_db_username.value
  db_password             = aws_ssm_parameter.addresses_postgres_db_password.value
}

module "address-es-dms-local-addresses" {
  source                       = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_replication_task"
  environment_name             = "development"
  project_name                 = "addresses-api"
  migration_type               = "full-load"
  replication_instance_arn     = aws_ssm_parameter.dms_rep_instance_arn.value
  replication_task_indentifier = "addresses-api-es-dms-task-local-addresses"
  task_settings                = file("${path.module}/task_settings.json")
  source_endpoint_arn          = module.source_db_endpoint.dms_endpoint_arn
  target_endpoint_arn          = aws_dms_endpoint.address_elasticsearch.endpoint_arn
  task_table_mappings          = file("${path.module}/selection_rules_local.json")
}

module "address-es-dms-national-addresses" {
  source                       = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_replication_task"
  environment_name             = "development"
  project_name                 = "addresses-api"
  migration_type               = "full-load"
  replication_instance_arn     = aws_ssm_parameter.dms_rep_instance_arn.value
  replication_task_indentifier = "addresses-api-es-dms-task-national-addresses"
  task_settings                = file("${path.module}/task_settings.json")
  source_endpoint_arn          = module.source_db_endpoint.dms_endpoint_arn
  target_endpoint_arn          = aws_dms_endpoint.address_elasticsearch.endpoint_arn
  task_table_mappings          = file("${path.module}/selection_rules_national.json")
}
