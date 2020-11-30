# INSTRUCTIONS:
# 1) ENSURE YOU POPULATE THE LOCALS
# 2) ENSURE YOU REPLACE ALL INPUT PARAMETERS, THAT CURRENTLY STATE 'ENTER VALUE', WITH VALID VALUES
# 3) YOUR CODE WOULD NOT COMPILE IF STEP NUMBER 2 IS NOT PERFORMED!
# 4) ENSURE YOU CREATE A BUCKET FOR YOUR STATE FILE AND YOU ADD THE NAME BELOW - MAINTAINING THE STATE OF THE INFRASTRUCTURE YOU CREATE IS ESSENTIAL - FOR APIS, THE BUCKETS ALREADY EXIST
# 5) THE VALUES OF THE COMMON COMPONENTS THAT YOU WILL NEED ARE PROVIDED IN THE COMMENTS
# 6) IF ADDITIONAL RESOURCES ARE REQUIRED BY YOUR API, ADD THEM TO THIS FILE
# 7) ENSURE THIS FILE IS PLACED WITHIN A 'terraform' FOLDER LOCATED AT THE ROOT PROJECT DIRECTORY

provider "aws" {
  region  = "eu-west-2"
  version = "~> 2.0"
}
data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

locals {
  parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}


terraform {
  backend "s3" {
    bucket  = "terraform-state-staging-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/addresses-api/state"
  }
}

/*    VPC SET UP    */

data "aws_vpc" "staging_vpc" {
  tags = {
    Name = "vpc-staging-apis-staging"
  }
}

data "aws_subnet_ids" "staging" {
  vpc_id = data.aws_vpc.staging_vpc.id
  filter {
    name   = "tag:Type"
    values = ["private"]
  }
}

/*    POSTGRES SET UP    */

data "aws_ssm_parameter" "addresses_postgres_db_password" {
  name = "/addresses-api/staging/postgres-password"
}

data "aws_ssm_parameter" "addresses_postgres_username" {
  name = "/addresses-api/staging/postgres-username"
}

data "aws_ssm_parameter" "addresses_postgres_hostname" {
  name = "/addresses-api/staging/postgres-hostname"
}

module "postgres_db_staging" {
  source               = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres"
  environment_name     = "staging"
  vpc_id               = data.aws_vpc.staging_vpc.id
  db_identifier        = "addresses-api"
  db_name              = "addresses_api"
  db_port              = 5502
  subnet_ids           = data.aws_subnet_ids.staging.ids
  db_engine            = "postgres"
  db_engine_version    = "11.8"
  db_instance_class    = "db.t2.micro"
  db_allocated_storage = 80
  maintenance_window   = "sun:10:00-sun:10:30"
  db_username          = data.aws_ssm_parameter.addresses_postgres_username.value
  db_password          = data.aws_ssm_parameter.addresses_postgres_db_password.value
  storage_encrypted    = false
  multi_az             = false //only true if production deployment
  publicly_accessible  = false
  project_name         = "platform apis"
}


/*    ELASTICSEARCH SETUP    */

module "elasticsearch_db_staging" {
  source           = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/elasticsearch"
  vpc_id           = data.aws_vpc.staging_vpc.id
  environment_name = "staging"
  port             = 443
  domain_name      = "addresses-api-es"
  subnet_ids       = [tolist(data.aws_subnet_ids.staging.ids)[0]]
  project_name     = "addresses-api"
  es_version       = "7.8"
  encrypt_at_rest  = "false"
  instance_type    = "t2.small.elasticsearch"
  ebs_enabled      = "true"
  ebs_volume_size  = "10"
  region           = data.aws_region.current.name
  account_id       = data.aws_caller_identity.current.account_id
}

data "aws_ssm_parameter" "addresses_elasticsearch_domain" {
  name = "/addresses-api/staging/elasticsearch-domain"
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
    name        = "DMS_Elasticsearch_Addresses_API"
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
            "Resource": "${module.elasticsearch_db_staging.es_arn}"
        }
    ]
}
EOF
}

resource "aws_iam_role_policy_attachment" "attach_policy" {
    role       = aws_iam_role.dms_service_role.name
    policy_arn = aws_iam_policy.es_policy.arn
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
    Environment  = "staging",
    project_name = "addresses-api"
  }
}

module "source_db_endpoint" {
  source                  = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_endpoint"
  database_name           = "addresses_api"
  dms_endpoint_identifier = "source-addresses-postgres"
  endpoint_type           = "source"
  engine_name             = "postgres"
  database_port           = 5502
  db_server               = data.aws_ssm_parameter.addresses_postgres_hostname.value
  ssl_mode                = "none"
  environment_name        = "staging"
  project_name            = "addresses-api"
  db_username             = data.aws_ssm_parameter.addresses_postgres_username.value
  db_password             = data.aws_ssm_parameter.addresses_postgres_db_password.value
}

module "address-es-dms" {
  source                       = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_replication_task"
  environment_name             = "staging"
  project_name                 = "addresses-api"
  migration_type               = "full-load-and-cdc"
  replication_instance_arn     = "arn:aws:dms:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:rep:DNTOW6TGQEGCAOWQMZYHQRTWAA"
  replication_task_indentifier = "addresses-api-es-dms-task"
  task_settings                = file("${path.module}/task_settings.json")
  source_endpoint_arn          = module.source_db_endpoint.dms_endpoint_arn
  target_endpoint_arn          = aws_dms_endpoint.address_elasticsearch.endpoint_arn
  task_table_mappings          = file("${path.module}/selection_rules.json")
}
