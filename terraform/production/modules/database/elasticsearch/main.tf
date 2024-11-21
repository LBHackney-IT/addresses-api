module "db_security_group" {
  source = "../../security_groups/database/internal_only_traffic"
  vpc_id = var.vpc_id
  db_name = var.domain_name
  db_port = var.port
  environment_name = var.environment_name
}

resource "aws_iam_service_linked_role" "es" {
    count = var.create_service_role ? 1 : 0
    aws_service_name = "es.amazonaws.com"
}

resource "aws_elasticsearch_domain" "lbh_es" {
    domain_name = var.domain_name
    elasticsearch_version = var.es_version

    encrypt_at_rest {
        enabled = var.encrypt_at_rest
    }
    vpc_options {
        subnet_ids = var.subnet_ids
        security_group_ids = [module.db_security_group.db_sg_id]
    }
      access_policies = <<CONFIG
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Action": "es:*",
            "Principal": "*",
            "Effect": "Allow",
            "Resource": "arn:aws:es:${var.region}:${var.account_id}:domain/${var.domain_name}/*"
        }
    ]
}
CONFIG
    cluster_config {
        instance_type = var.instance_type
        instance_count = var.instance_count
        zone_awareness_enabled = var.zone_awareness_enabled
        zone_awareness_config {
          availability_zone_count = var.availability_zone_count
        }
    }
    ebs_options {
        ebs_enabled = var.ebs_enabled
        volume_size = var.ebs_volume_size
    }
    log_publishing_options {
        cloudwatch_log_group_arn = "arn:aws:logs:eu-west-2:153306643385:log-group:/aws/aes/domains/addresses-api-es/application-logs"
        enabled                  = true
        log_type                 = "ES_APPLICATION_LOGS"
    }
    log_publishing_options {
        cloudwatch_log_group_arn = "arn:aws:logs:eu-west-2:153306643385:log-group:/aws/aes/domains/addresses-api-es/search-logs"
        enabled                  = true
        log_type                 = "SEARCH_SLOW_LOGS"
    }

    snapshot_options {
        automated_snapshot_start_hour = 23
    }
    tags = {
        Name = "${var.domain_name}-${var.environment_name}"
        Environment = var.environment_name
        terraform-managed = true
        project_name = var.project_name
    }
    depends_on = [aws_iam_service_linked_role.es]
}
