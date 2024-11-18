variable "vpc_id" {
  type = string
}
variable "db_identifier" {
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
variable "subnet_ids" {
  type = list(string)
}
variable "db_engine" {
  type = string
}
variable "db_engine_version" {
  type = string
}
variable "db_instance_class" {
  type = string
}
variable "db_allocated_storage" {
  type = string
}
variable "db_max_allocated_storage" {
  description = "[optional] Enables storage autoscaling if set to be higher than 'db_allocated_storage'. Will scale to the specified number in GB."
  type        = number
  default     = 0
}
variable "maintenance_window" {
  type = string //e.g. "tue:10:00-tue:10:30"
}
variable "db_username" {
  type = string
}
variable "db_password" {
  type = string
}
variable "storage_encrypted" {
  type = string
}
variable "multi_az" {
  type = string
}
variable "publicly_accessible" {
  type = string
}
variable "project_name" {
  type = string
}
variable "additional_tags" {
  description = "[optional] Extra tags that will be appended to the module tags."
  type        = map(string)
  default     = { }
}
variable "db_parameter_group_name" {
  type = string
  default = null
}
variable "db_allow_major_version_upgrade" {
  type    = string
  default = "false"
}
variable "copy_tags_to_snapshot" {
  description = "[optional] Copy the database instance tags onto the snapshot. Makes it easier to preserve instance tags on backup -> restore."
  type = bool
  default = false
}
variable "deletion_protection" {
  description = "[optional] Database cannot be deleted while this is set to 'true'. Prevents accidental deletions due TF 'replace'."
  type = bool
  default = false
}
