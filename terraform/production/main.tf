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
    bucket  = "terraform-state-production-apis"
    encrypt = true
    region  = "eu-west-2"
    key     = "services/addresses-api/state"
  }
}
/*    VPC SET UP    */

data "aws_vpc" "production_vpc" {
  tags = {
    Name = "vpc-production-apis-production"
  }
}

data "aws_subnet_ids" "production" {
  vpc_id = data.aws_vpc.production_vpc.id
  filter {
    name   = "tag:Type"
    values = ["private"]
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

module "postgres_db_production" {
  source               = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/postgres"
  environment_name     = "production"
  vpc_id               = data.aws_vpc.production_vpc.id
  db_identifier        = "addresses-api"
  db_name              = "addresses_api"
  db_port              = 5500
  subnet_ids           = data.aws_subnet_ids.production.ids
  db_engine            = "postgres"
  db_engine_version    = "11.8"
  db_instance_class    = "db.t3.micro"
  db_allocated_storage = 100
  maintenance_window   = "sun:10:00-sun:10:30"
  db_username          = data.aws_ssm_parameter.addresses_postgres_username.value
  db_password          = data.aws_ssm_parameter.addresses_postgres_db_password.value
  storage_encrypted    = true
  multi_az             = true //only true if production deployment
  publicly_accessible  = false
  project_name         = "platform apis"
}

/*    CLOUDWATCH ALARM SET UP    */

data "aws_sns_topic" "platform_apis" {
  name = "Platform-APIs-Alerts"
}

resource "aws_cloudwatch_metric_alarm" "database-transactions-log-disk-usage" {
  alarm_name          = "database-transactions-log-disk-usage"
  alarm_description   = "This metric monitors transactions log disk usage goes above a threshold"
  alarm_actions       = ["${data.aws_sns_topic.platform_apis.arn}"]
  comparison_operator = "GreaterThanThreshold"
  threshold           = "3000"
  evaluation_periods  = "1"
  metric_name         = "TransactionLogsDiskUsage"
  namespace           = "RDS"
  period              = "300"
  statistic           = "Average"

  dimensions = {
    DBInstanceIdentifier = "${module.postgres_db_production.instance_id}"
  }
}

resource "aws_cloudwatch_metric_alarm" "free_storage_space_too_low" {
  alarm_name          = "free-storage-space-too-low"
  alarm_description   = "Average database free storage space too low within the last 5 minutes"
  alarm_actions       = ["${data.aws_sns_topic.platform_apis.arn}"]
  comparison_operator = "LessThanThreshold"
  threshold           = "40000"
  evaluation_periods  = "1"
  metric_name         = "FreeStorageSpace"
  namespace           = "RDS"
  period              = "300"
  statistic           = "Average"

  dimensions = {
    DBInstanceIdentifier = "${module.postgres_db_production.instance_id}"
  }
}

/*    ELASTICSEARCH SETUP    */

module "elasticsearch_db_production" {
  source           = "github.com/LBHackney-IT/aws-hackney-common-terraform.git//modules/database/elasticsearch"
  vpc_id           = data.aws_vpc.production_vpc.id
  environment_name = "production"
  port             = 443
  domain_name      = "addresses-api-es"
  subnet_ids       = [tolist(data.aws_subnet_ids.production.ids)[0]]
  project_name     = "addresses-api"
  es_version       = "7.8"
  encrypt_at_rest  = "true"
  instance_type    = "t3.small.elasticsearch"
  ebs_enabled      = "true"
  ebs_volume_size  = "10"
  region           = data.aws_region.current.name
  account_id       = data.aws_caller_identity.current.account_id
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

module "address-es-dms" {
  source                       = "github.com/LBHackney-IT/aws-dms-terraform.git//dms_replication_task"
  environment_name             = "production"
  project_name                 = "addresses-api"
  migration_type               = "full-load-and-cdc"
  replication_instance_arn     = "arn:aws:dms:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:rep:65CJ5HE2DMCUW5X6EPKTKUDVWA"
  replication_task_indentifier = "addresses-api-es-dms-task"
  task_settings                = file("${path.module}/task_settings.json")
  source_endpoint_arn          = module.source_db_endpoint.dms_endpoint_arn
  target_endpoint_arn          = aws_dms_endpoint.address_elasticsearch.endpoint_arn
  task_table_mappings          = file("${path.module}/selection_rules.json")
}
