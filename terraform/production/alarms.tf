/*    CLOUDWATCH ALARM SET UP    */

data "aws_sns_topic" "platform_apis" {
  name = "Platform-APIs-Alerts"
}

resource "aws_cloudwatch_metric_alarm" "address_api_db_transactions_log_disk_usage" {
  alarm_name          = "address-api-db-transactions-log-disk-usage-too-high"
  alarm_description   = "Average database transactions log disk usage higher than threshold in the last 5 minutes"
  alarm_actions       = ["${data.aws_sns_topic.platform_apis.arn}"]
  comparison_operator = "GreaterThanThreshold"
  threshold           = "40000" # Unit: MB
  evaluation_periods  = "1"
  metric_name         = "TransactionLogsDiskUsage"
  namespace           = "RDS"
  period              = "300"
  statistic           = "Average"

  dimensions = {
    DBInstanceIdentifier = "${module.postgres_db_production.instance_id}"
  }
}

resource "aws_cloudwatch_metric_alarm" "address_api_db_free_storage_space" {
  alarm_name          = "address-api-db-free-storage-space-too-low"
  alarm_description   = "Average database free storage space log lower than threshold in the last 5 minutes"
  alarm_actions       = ["${data.aws_sns_topic.platform_apis.arn}"]
  comparison_operator = "LessThanThreshold"
  threshold           = "40000" # Unit: MB
  evaluation_periods  = "1"
  metric_name         = "FreeStorageSpace"
  namespace           = "RDS"
  period              = "300"
  statistic           = "Average"

  dimensions = {
    DBInstanceIdentifier = "${module.postgres_db_production.instance_id}"
  }
}
