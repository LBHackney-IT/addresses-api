variable "environment_name" {
  type = string
}

variable "project_name" {
  type = string
}

variable "replication_instance_class" {
  type = string //e.g. "dms.t2.small"
}

variable "replication_instance_identifier" {
  type = string
}

variable "maintenance_window" {
  type = string //e.g. "tue:10:00-tue:10:30"
}

variable "subnet_ids" {
  type = list(string)
}

variable "vpc_security_group_ids" {
  description = "A list of security group IDs to associate with"
  type        = list(string)
  default     = null
}

variable "vpc_id" {
  type = string
}
