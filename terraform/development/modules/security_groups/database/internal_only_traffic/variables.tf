variable "vpc_id" {
  type = string
}
variable "db_name" {
  type = string
}
variable "environment_name" {
  type = string
}
variable "db_port" {
  type = string
}
variable "bastion_security_group_id" {
  description = "Security group ID of the bastion host allowed to connect to the database"
  type        = string
}
