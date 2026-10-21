# Manually kicking off re-indexing

The re-indexing process should happen every night, as everything is re-enabled.

But in case the process needs to be triggered manually, the instructions are below:

To manually kick off the overnight re-indexing:

- locate the 'addresses-api-reindex-es-alias-production' lambda in AWS
- click on the 'Test' tab
- I have set up 2 saved events 'TestReindexHackney' and 'TestReindexLocal'
- Run both of these test events, by selecting them and clicking on the orange 'Test' button
- Each re-indexing should be finished within 15 minutes

## Manual national addresses index swap

The national addresses re-indexing takes a long time and can hit the max 50 attempts limit. When that happens you'll see the following message in the Lambda's CloudWatch logs:

`Too many attempts have been detected - maximum number is 50. Terminating to prevent possible message build up
`

Easiest way to run the commands below is to use session manager. You can set the `$ES` variable first with:

```bash
ES=$(aws ssm get-parameter --name /addresses-api/<environment>/elasticsearch-domain --query Parameter.Value --output text --region=eu-west-2)
``` 

First check that re-indexing job has completed with `curl -s "$ES/_tasks/<nodeId>:<taskId>"`.
nodeId and taskId are visible in the logs inside the `Received SQS message...` message

Then swap the index over manually with (replace `<old>` and `<new>` with appropriate timestamps):

```bash
curl -s -X POST "$ES/_aliases" -H 'Content-Type: application/json' -d '{
  "actions": [
    { "remove": { "index": "national_addresses_<old>", "alias": "national_addresses" } },
    { "add":    { "index": "national_addresses_<new>", "alias": "national_addresses" } }
  ]
}'
```


and then delete the old index with:

```bash
 curl -s -X DELETE "$ES/national_addresses_<old>"
```

Deletion is important to keep the disk usage in check.

If you are running re-indexing when there is no existing national_addresses index you can simply create the new alias with:

```bash
 curl -s -X PUT "$ES/national_addresses_<new>/_alias/national_addresses"
```

