module "db_security_group" {
  source                    = "../../security_groups/database/internal_only_traffic"
  vpc_id                    = var.vpc_id
  db_name                   = var.db_name
  db_port                   = var.db_port
  environment_name          = var.environment_name
  bastion_security_group_id = var.bastion_security_group_id
}

resource "aws_db_subnet_group" "db_subnets" {
  name       = "${var.db_name}-db-subnet-${var.environment_name}"
  subnet_ids = var.subnet_ids

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_db_instance" "lbh_db" {
  identifier                  = var.db_identifier
  engine                      = "postgres"
  engine_version              = var.db_engine_version
  instance_class              = var.db_instance_class
  allocated_storage           = var.db_allocated_storage
  max_allocated_storage       = var.db_max_allocated_storage
  ca_cert_identifier          = "rds-ca-rsa2048-g1"
  storage_type                = "gp2"
  port                        = var.db_port
  maintenance_window          = var.maintenance_window
  backup_window               = "00:01-00:31"
  username                    = var.db_username
  password                    = var.db_password
  vpc_security_group_ids      = [module.db_security_group.db_sg_id]
  db_subnet_group_name        = aws_db_subnet_group.db_subnets.name
  db_name                     = var.db_name
  monitoring_interval         = var.monitoring_interval
  backup_retention_period     = 30
  storage_encrypted           = true
  kms_key_id                  = var.kms_key_id
  multi_az                    = var.multi_az
  auto_minor_version_upgrade  = true
  allow_major_version_upgrade = var.db_allow_major_version_upgrade
  parameter_group_name        = var.db_parameter_group_name

  apply_immediately   = false
  publicly_accessible = var.publicly_accessible

  # Deletion / Restore related
  deletion_protection   = var.deletion_protection
  skip_final_snapshot   = true
  copy_tags_to_snapshot = var.copy_tags_to_snapshot

  tags = merge(
    var.additional_tags,
    {
      Name              = "${var.db_name}-db-${var.environment_name}"
      Environment       = var.environment_name
      terraform-managed = true
      project_name      = var.project_name
    }
  )
}
