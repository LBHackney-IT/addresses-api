resource "aws_security_group" "dms_sg" {
  name        = "dms-instance-${var.environment_name}"
  description = "Allow self traffic for DMS security group"
  vpc_id      = var.vpc_id

  ingress {
    description = "DMS traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    self = true
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "dms-instance-${var.environment_name}-sg"
  }
}
