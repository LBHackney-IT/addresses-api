output "es_arn" {
  value = aws_elasticsearch_domain.lbh_es.arn
}
output "es_endpoint_url" {
  value = aws_elasticsearch_domain.lbh_es.endpoint
}
output "security_group_id" {
  value = module.db_security_group.db_sg_id
}
