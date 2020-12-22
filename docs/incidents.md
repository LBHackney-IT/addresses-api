# Incidents 

This is a living document which describes some of the known issues we've had with the v2 API, and what to look for. 

## Postgres running out of disk space 

We've previously seen this as a result of a stopped/failed DMS task causing the Postgres Replication Slot to fill up and consume remaining disk space. 

**Resolution:** Increase Postgres disk space slightly (20GB) and restart DMS task to clear the backlog. 

**Remeidal action:**
- Disk auto-scaling in Production 
- TODO: more granular and sensetive alerting to resource constriction 

## ElasticSearch failing to index 

This usually manifests itself quite dramatically with the API falling over as shards become unavailable and newly indexed data isn't made available to the API. 

This will also cause the DMS/ES Lambas to fail to run properly. 

**Resolution:** scale up ES cluster and manually trigger new reindex of data (through Lambda test event in Console). Keep eyes on CloudWatch. 

**Remidial action:**
- TODO: more granular and sensetive alerting to resource constriction 
