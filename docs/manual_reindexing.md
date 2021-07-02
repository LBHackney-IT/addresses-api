## Manually kicking off reindexing

The reindexing process should happen every night, as everything is re-enabled.

But in case the process needs to be triggered manually, the instructions are below:

To manually kick off the overnight re-indexing:
 - locate the 'addresses-api-reindex-es-alias-production' lambda in AWS
 - click on the 'Test' tab
 - I have set up 2 saved events 'TestReindexHackney' and 'TestReindexLocal'
 - Run both of these test events, by selecting them and clicking on the orange 'Test' button
 - Each reindexing should be finished within 15 minutes
