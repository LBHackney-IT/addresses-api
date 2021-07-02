## Troubleshooting AWS elasticsearch issues

If there is an issue with elasticsearch, you should be able to see it by checking the cluster health in AWS.

If the cluster is not accepting writes, it is probably because there is no spare storage capacity.
If the re-indexing process fails, it can leave orphaned indexes that take up disk space. This shouldn't really happen (after July 2021) as it was fixed, but just in case this is how you fix it:
1. Go to kibana (you will need to be on the AWS VPC to so this, Sandrine Balley or Matt Keyworth can help with this). You can find the Kibana URL by klooking at the elasticsearch instance in AWS.
2. List all indexes, and note the ones that are named *hackney_addresses_[timestamp]* or *national_addresses_[timestamp]* - i.e. the ones that end with a timestamp. Ignore the 'base' indexes named *hackney_address* and *local_address*
3. Open a dev session in Kibana, and for all the indexes that end with a timestamp, _except_ the most recent, delete them. The command to delete an index is e.g.
 `DELETE /national_addresses_202105120200`
4. When finished, there should only be 4 indices - *hackney_address*, *local_address*, *hackney_addresses_[most-recent-timestamp]*, *national_addresses_[most-recent-timestamp]*

