output "instance_identifier" {
  value = aws_db_instance.lbh-db.identifier
}

output "security_group_id" {
  value = module.db_security_group.db_sg_id
}
