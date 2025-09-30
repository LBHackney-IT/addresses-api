module "dms_replication_instance_sg" {
  source = "../security_groups/dms"
  environment_name = "staging"
  vpc_id = var.vpc_id
}

resource "aws_dms_replication_subnet_group" "dms_subnet_group" {
  replication_subnet_group_description = "Replication subnet group for ${var.project_name}"
  replication_subnet_group_id          = "dms-replication-subnet-group-${var.replication_instance_identifier}"

  subnet_ids = var.subnet_ids


  tags = {
    Environment = var.environment_name,
    project_name = var.project_name
  }
}

resource "aws_dms_replication_instance" "address_dms_rep_instance" {
  replication_instance_class = var.replication_instance_class
  replication_instance_id    = var.replication_instance_identifier
  replication_subnet_group_id = aws_dms_replication_subnet_group.dms_subnet_group.id
  engine_version = "3.5.3"
  auto_minor_version_upgrade = false
  publicly_accessible = false
  allocated_storage = 20
  availability_zone = "eu-west-2a"
  multi_az = false
  apply_immediately = true

  tags = {
    Name = var.replication_instance_identifier
    Environment = var.environment_name
    project_name = var.project_name
  }

  vpc_security_group_ids = [module.dms_replication_instance_sg.dms_sg_id]
}