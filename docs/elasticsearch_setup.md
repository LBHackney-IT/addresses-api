
# Elasticsearch setup 
## Overview
Our elasticsearch cluster is hosted on AWS Elastisearch Service.

We use AWS Database Migration Service (DMS) with the Address API Postgres [RDS as a source](https://docs.aws.amazon.com/dms/latest/userguide/CHAP_Source.PostgreSQL.html) and the [Elasticsearch Cluster as a target](https://docs.aws.amazon.com/dms/latest/userguide/CHAP_Target.Elasticsearch.html) to migrate data into the cluster. DMS is setup using Change Data Capture (CDC) so will migrate changes accross continuously.

The DMS setup in managed in terrform. You can find the terraform folder at the root of this repository.

DMS is pointed at two source tables `dbo.hackney_address` and `dbo.national_address` for each it will create and manage an index in elasticsearch called `hackney_address` and `national_address` respectively. DMS has full control over these indices so we can not edit the [field mappings](https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping.html) and need to close the indexes in order to add [custom analyzers](https://www.elastic.co/guide/en/elasticsearch/reference/7.10/analysis-custom-analyzer.html) to the indices. 

As we have no control over the index settings we can't use these indices to directly search against. To solve this we have a lambda function, running on a schedule, that reindexes the data. 

This lambda function can also be run manually if needed. For example, If you need to change the index settings you could edit the [index file](../data/elasticsearch/index.json), PR and merge the change and then run this lambda function for each index to apply the changes to the indices.

At some point, it would be nice to have this running in the pipeline, when the index file has been editted. 

## Reindexing Lambdas

The first lambda function runs on a schedule, you can find the definition for the lambda function in the [serverless file](../serverless.yml#L30) including the location of the entrypoint in the code.

It takes the following input
```json
{
  "alias": "hackney_addresses",
  "fromIndex": "hackney_address",
  "config": "{
    \"mappings\": {
      \"postcode\": {
        \"type\": \"text\"
      }
    }
  }",
  "deleteAfterReindex": true
}
```

- `alias` Is the alias name you want to be assigned to the newly created index. It should be the alias that you search against. In our case it will be `hackney_addresses` or `national_addresses`.
- `fromIndex` (Optional) is the index that you want to move data from into the new index.
>Note
>  - When running this on the schedule to copy data from the DMS created indices into the managed indices, `fromIndex` should be set to `hackney_address` or `national_address` (the names of the DMS created indices).
>  - When making changes to the Address API that require making changes to the index configuration you can run this lambda with `fromIndex` empty and it will copy the data from the previously index into one with new settings. 
- `config` (Optional) is a json containing the settings for the new index.
- `deleteAfterReindex` (Optional) is a boolean flag, defaulting to `false`. Whether, or not, to delete indexes attached to the alias. For the purposes of this project we would usually want it set to true so as not to take up uneccessary space. 

1. If no `fromIndex` value is supplied, it will get the index name associated the the alias provided and use that as the index to transfer data from.
2. Creates an index with the provided `config` json, or if the config json is not supplied it will use the [index file](../data/elasticsearch/index.json) committed into this repository. The name will be the alias and a timestamp, for example `hackney_addresses_202012011230`.
3. Will start [reindexing](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/reindexing-documents.html#reindex) the data on the elastisearch server. This will create a [task](https://www.elastic.co/guide/en/elasticsearch/reference/current/tasks.html) in elasticsearch and return the task ID.
4. Send a message to an SQS queue with the task ID, alias name, new index name, and `deleteAfterReindex`.

The second lambda function is triggered by messages being added to the SQS queue, you can find the definition for the lambda function in the [serverless file](../serverless.yml#L40) including the location of the entrypoint in the code. It performs the following steps.

1. It will get the task details from elasticsearch using the task ID in the SQS queue message,
2. If the task can't be found it will return without doing anything
3. If the task can be found but is still in process it will add the sqs message back to the queue.
4. If the task has completed, all indexes are removed from the alias.
5. The alias is added to the new index that has just been created and populated with data.
6. The task document is deleted from elasticsearch.
7. If `deleteAfterReindex` is `true`, any indexes that were attached to the alias are deleted leaving just the newly created index. Note that if the `fromIndex` index does not have the `alias` attached to it then it won't be deleted.
