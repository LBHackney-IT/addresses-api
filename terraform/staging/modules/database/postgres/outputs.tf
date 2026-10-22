output "instance_id" {
  value = aws_db_instance.lbh-db.id
}

output "db_instance_endpoint" {
  description = "The hostname of the RDS instance (endpoint URL) that can be used to create SSM parameter"
  value       = aws_db_instance.lbh-db.address
}

output "db_security_group_id" {
  description = "Security group id of the postgres RDS database instance."
  value       = module.db_security_group.db_sg_id
}
