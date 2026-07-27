output "instance_endpoint" {
  value = aws_db_instance.lbh_db.address
}

output "security_group_id" {
  value = module.db_security_group.db_sg_id
}
