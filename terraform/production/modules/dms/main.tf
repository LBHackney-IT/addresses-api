resource "aws_dms_replication_subnet_group" "dms_subnet_group" {
  replication_subnet_group_description = "Replication subnet group for ${var.project_name}"
  replication_subnet_group_id          = var.replication_subnet_group_id

  subnet_ids = var.subnet_ids

  tags = {
    Environment  = var.environment_name,
    project_name = var.project_name
  }
}

resource "aws_dms_replication_instance" "address_dms_rep_instance" {
  replication_instance_class   = var.replication_instance_class
  replication_instance_id      = var.replication_instance_identifier
  replication_subnet_group_id  = aws_dms_replication_subnet_group.dms_subnet_group.id
  preferred_maintenance_window = var.maintenance_window
  engine_version               = var.engine_version
  auto_minor_version_upgrade   = var.auto_minor_version_upgrade
  publicly_accessible          = false
  allocated_storage            = var.allocated_storage
  availability_zone            = var.availability_zone
  multi_az                     = var.multi_az
  apply_immediately            = true

  tags = {
    Name         = var.replication_instance_identifier
    Environment  = var.environment_name
    project_name = var.project_name
  }

  vpc_security_group_ids = var.vpc_security_group_ids

  # Imported live instance. availability_zone, publicly_accessible, and
  # replication_subnet_group_id force replacement — align those with AWS
  # before apply.
  lifecycle {
    prevent_destroy = true
  }
}
