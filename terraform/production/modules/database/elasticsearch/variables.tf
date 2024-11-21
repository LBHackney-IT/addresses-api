
variable "vpc_id" {
  type = string
}
variable "environment_name" {
  type = string
}
variable "port" {
  type = string
}
variable "domain_name" {
  type = string
}
variable "subnet_ids" {
  type = list
}
variable "project_name" {
  type = string
}
variable "es_version" {
  type = string
}
variable "encrypt_at_rest" {
  type = string
}
variable "instance_type" {
  type = string
}
variable "instance_count" {
  type = string
}
variable "ebs_enabled" {
  type = string
}
variable "ebs_volume_size" {
  type = string
}
variable "region" {
  type = string
}
variable "account_id" {
  type = string
}
variable "create_service_role" {
  type = bool
  default = true
}
variable "zone_awareness_enabled" {
  type = bool
  default = false
}
variable "availability_zone_count" {
  type = number
  default = 2
}