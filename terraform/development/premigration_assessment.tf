# DMS premigration assessment report storage
# Used when creating a premigration assessment from the DMS task Actions menu.
# Docs: https://docs.aws.amazon.com/dms/latest/userguide/CHAP_Tasks.AssessmentReport.Prerequisites.html

locals {
  dms_assessment_bucket_name = "addresses-api-dms-premigration-${data.aws_caller_identity.current.account_id}-development"
  dms_assessment_role_name   = "addresses-api-dms-premigration-assessment-development"
  dms_assessment_role_arn    = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${local.dms_assessment_role_name}"
}

resource "aws_kms_key" "dms_premigration_assessment" {
  description             = "KMS key for Addresses API DMS premigration assessment S3 bucket (development)"
  deletion_window_in_days = 7
  enable_key_rotation     = true
  policy                  = data.aws_iam_policy_document.dms_premigration_assessment_kms.json

  tags = {
    Name         = "addresses-api-dms-premigration-assessment-development"
    Environment  = "development"
    project_name = "addresses-api"
  }
}

resource "aws_kms_alias" "dms_premigration_assessment" {
  name          = "alias/addresses-api-dms-premigration-assessment-development"
  target_key_id = aws_kms_key.dms_premigration_assessment.key_id
}

data "aws_iam_policy_document" "dms_premigration_assessment_kms" {
  statement {
    sid    = "EnableRootAccountAdministration"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${data.aws_caller_identity.current.account_id}:root"]
    }

    actions   = ["kms:*"]
    resources = ["*"]
  }

  statement {
    sid    = "AllowDmsAssessmentRoleUseOfTheKey"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = [local.dms_assessment_role_arn]
    }

    actions = [
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:GenerateDataKey*",
      "kms:DescribeKey",
    ]
    resources = ["*"]
  }

  statement {
    sid    = "AllowS3UseOfTheKey"
    effect = "Allow"

    principals {
      type        = "Service"
      identifiers = ["s3.amazonaws.com"]
    }

    actions = [
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:GenerateDataKey*",
      "kms:DescribeKey",
    ]
    resources = ["*"]

    condition {
      test     = "StringEquals"
      variable = "kms:ViaService"
      values   = ["s3.${data.aws_region.current.name}.amazonaws.com"]
    }

    condition {
      test     = "StringEquals"
      variable = "kms:CallerAccount"
      values   = [data.aws_caller_identity.current.account_id]
    }
  }
}

resource "aws_s3_bucket" "dms_premigration_assessment" {
  bucket = local.dms_assessment_bucket_name

  tags = {
    Name         = local.dms_assessment_bucket_name
    Environment  = "development"
    project_name = "addresses-api"
    Purpose      = "dms-premigration-assessment-reports"
  }
}

resource "aws_s3_bucket_ownership_controls" "dms_premigration_assessment" {
  bucket = aws_s3_bucket.dms_premigration_assessment.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_public_access_block" "dms_premigration_assessment" {
  bucket = aws_s3_bucket.dms_premigration_assessment.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_versioning" "dms_premigration_assessment" {
  bucket = aws_s3_bucket.dms_premigration_assessment.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "dms_premigration_assessment" {
  bucket = aws_s3_bucket.dms_premigration_assessment.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm     = "aws:kms"
      kms_master_key_id = aws_kms_key.dms_premigration_assessment.arn
    }
    bucket_key_enabled = true
  }
}

data "aws_iam_policy_document" "dms_premigration_assessment_bucket" {
  statement {
    sid    = "DenyInsecureTransport"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions = ["s3:*"]
    resources = [
      aws_s3_bucket.dms_premigration_assessment.arn,
      "${aws_s3_bucket.dms_premigration_assessment.arn}/*",
    ]

    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values   = ["false"]
    }
  }

  statement {
    sid     = "AllowSSLRequestsOnly"
    effect  = "Deny"
    actions = ["s3:*"]
    principals {
      type        = "AWS"
      identifiers = ["*"]
    }
    resources = [
      aws_s3_bucket.dms_premigration_assessment.arn,
      "${aws_s3_bucket.dms_premigration_assessment.arn}/*",
    ]
    condition {
      test     = "Bool"
      variable = "aws:SecureTransport"
      values = [
        "false"
      ]
    }
  }

  statement {
    sid    = "DenyWrongKmsKey"
    effect = "Deny"

    principals {
      type        = "*"
      identifiers = ["*"]
    }

    actions   = ["s3:PutObject"]
    resources = ["${aws_s3_bucket.dms_premigration_assessment.arn}/*"]

    condition {
      test     = "StringNotEquals"
      variable = "s3:x-amz-server-side-encryption-aws-kms-key-id"
      values   = [aws_kms_key.dms_premigration_assessment.arn]
    }
  }

  statement {
    sid    = "AllowAccountRootRead"
    effect = "Allow"

    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${data.aws_caller_identity.current.account_id}:root"]
    }

    actions = [
      "s3:GetBucketLocation",
      "s3:ListBucket",
      "s3:GetObject",
      "s3:GetObjectVersion",
    ]
    resources = [
      aws_s3_bucket.dms_premigration_assessment.arn,
      "${aws_s3_bucket.dms_premigration_assessment.arn}/*",
    ]
  }
}

resource "aws_s3_bucket_policy" "dms_premigration_assessment" {
  bucket = aws_s3_bucket.dms_premigration_assessment.id
  policy = data.aws_iam_policy_document.dms_premigration_assessment_bucket.json

  depends_on = [aws_s3_bucket_public_access_block.dms_premigration_assessment]
}

resource "aws_iam_role" "dms_premigration_assessment" {
  name               = local.dms_assessment_role_name
  description        = "Role for DMS to write premigration assessment reports to S3"
  assume_role_policy = data.aws_iam_policy_document.dms-assume-role-policy.json

  tags = {
    Name         = local.dms_assessment_role_name
    Environment  = "development"
    project_name = "addresses-api"
  }
}

data "aws_iam_policy_document" "dms_premigration_assessment_role" {
  statement {
    sid    = "AllowAssessmentObjectAccess"
    effect = "Allow"
    actions = [
      "s3:PutObject",
      "s3:DeleteObject",
      "s3:GetObject",
      "s3:PutObjectTagging",
    ]
    resources = ["${aws_s3_bucket.dms_premigration_assessment.arn}/*"]
  }

  statement {
    sid    = "AllowAssessmentBucketAccess"
    effect = "Allow"
    actions = [
      "s3:ListBucket",
      "s3:GetBucketLocation",
    ]
    resources = [aws_s3_bucket.dms_premigration_assessment.arn]
  }

  statement {
    sid    = "AllowUseOfAssessmentKmsKey"
    effect = "Allow"
    actions = [
      "kms:Encrypt",
      "kms:Decrypt",
      "kms:ReEncrypt*",
      "kms:GenerateDataKey*",
      "kms:DescribeKey",
    ]
    resources = [aws_kms_key.dms_premigration_assessment.arn]
  }
}

resource "aws_iam_role_policy" "dms_premigration_assessment" {
  name   = "addresses-api-dms-premigration-assessment-s3-development"
  role   = aws_iam_role.dms_premigration_assessment.id
  policy = data.aws_iam_policy_document.dms_premigration_assessment_role.json
}

resource "aws_ssm_parameter" "dms_premigration_assessment_bucket" {
  name  = "/addresses-api/development/dms-premigration-assessment-bucket"
  type  = "String"
  value = aws_s3_bucket.dms_premigration_assessment.id
}

resource "aws_ssm_parameter" "dms_premigration_assessment_role_arn" {
  name  = "/addresses-api/development/dms-premigration-assessment-role-arn"
  type  = "String"
  value = aws_iam_role.dms_premigration_assessment.arn
}

resource "aws_ssm_parameter" "dms_premigration_assessment_kms_key_arn" {
  name  = "/addresses-api/development/dms-premigration-assessment-kms-key-arn"
  type  = "String"
  value = aws_kms_key.dms_premigration_assessment.arn
}
