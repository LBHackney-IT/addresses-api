# Connecting to AWS Resouces

The database on an RDS and elasticsearch cluster are both in a VPC and so can't be accessed from outside. 

If you need to access the database or make requests to elasticsearch then you can connect using AWS systems manager parameter store using one of the following methods. 

1. Using the command line of an EC2 instance that's inside the VPC. From here you can connect to the RDS instance using psql and make https requests to the elasticsearch domain. 
```sh
$ aws ssm $(aws ssm get-parameter --name /platform-apis-jump-box-instance-name --query Parameter.Value)
```
2. If you need to connect to the database in the a database manager or upload/ download data from your local machine you will need to use ssh with port forwarding. To do this you first need to save the private key file locally.

```sh
$ aws ssm get-parameter --name "/platform-apis-jump-box-pem-key" --output text --query Parameter.Value > ./private-key.pem
$ chmod 400 ./private-key.pem
```

Then you can connect using ssh, replace `<database-host-name>` and `<database-port>` with their correct values below. 

```sh
$ ssh -4 -v -i ./private-key.pem -Nf -M -L 5432:<database-host-name>:<database-port> -o "UserKnownHostsFile=/dev/null" -o "StrictHostKeyChecking=no" -o ProxyCommand="aws ssm start-session --target %h --document AWS-StartSSHSession --parameters portNumber=%p --region=eu-west-2" ec2-user@$(aws ssm get-parameter --name /platform-apis-jump-box-instance-name --query Parameter.Value)
```

Now you will be able to connect to the rds instance at 127.0.0.1:5432.