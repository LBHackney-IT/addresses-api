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
  parameter_store = "arn:aws:ssm:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:parameter"
}

//TODO: check if still needed
data "aws_iam_role" "ec2_container_service_role" {
  name = "ecsServiceRole"
}
//TODO: check if still needed
data "aws_iam_role" "ecs_task_execution_role" {
  name = "ecsTaskExecutionRole"
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

//TODO: username and password handling against restored db
//TODO: check port and storage configuration. We don't need that much storage. Maybe only 60GB
//TODO: change admin password
module "postgres_db_development" {
  source                   = "./modules/database/postgres"
  environment_name         = "development"
  vpc_id                   = data.aws_vpc.development_vpc.id
  db_identifier            = "addresses-api-db-development"
  db_name                  = "addresses_api"
  db_port                  = 5501
  subnet_ids               = data.aws_subnets.development.ids
  db_engine                = "postgres"
  db_engine_version        = "16.13"
  db_instance_class        = "db.t4g.small"
  db_allocated_storage     = 100
  db_max_allocated_storage = 0
  monitoring_interval      = 0
  maintenance_window       = "sun:11:00-sun:11:30"
  #db_username              = data.aws_ssm_parameter.addresses_postgres_username.value
  #db_password              = data.aws_ssm_parameter.addresses_postgres_db_password.value
  storage_encrypted     = true
  kms_key_id            = data.aws_kms_key.local_backup_key.arn
  multi_az              = false
  publicly_accessible   = false
  project_name          = "platform apis"
  deletion_protection   = true
  copy_tags_to_snapshot = true
  additional_tags = {
    BackupPolicy = "Dev"
  }
}
