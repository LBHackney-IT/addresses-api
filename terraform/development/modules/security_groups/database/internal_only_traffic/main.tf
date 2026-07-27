resource "aws_security_group" "lbh_db_traffic" {
  vpc_id      = var.vpc_id
  name_prefix = "allow_${var.db_name}_db_traffic"

  egress {
    description = "allow outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"

    cidr_blocks = [
      "0.0.0.0/0",
    ]
  }

  tags = {
    Name = "${var.db_name}-${var.environment_name}"
  }
}
