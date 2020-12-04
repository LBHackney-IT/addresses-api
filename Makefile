.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build addresses-api

.PHONY: serve
serve:
	docker-compose build addresses-api && docker-compose up addresses-api

.PHONY: shell
shell:
	docker-compose run addresses-api bash

.PHONY: test
test:
	docker-compose build addresses-api-test && docker-compose up -d test-elasticsearch && docker-compose up addresses-api-test

.PHONY: migrate-dev-database
migrate-dev-database:
	-dotnet tool install -g dotnet-ef
	cd AddressesAPI && CONNECTION_STRING="Host=127.0.0.1;Port=5433;Database=devdb;Username=postgres;Password=mypassword" dotnet ef database update

.PHONY: seed-pg-data
seed-pg-data:
	docker exec dev-database psql -d devdb -U postgres -f /var/seed-dev-data.sql

.PHONY: seed-es-data
seed-es-data:
	curl http://localhost:9200/addresses --data-binary "@./data/elasticsearch/index.json" -X PUT -s -H "Content-Type: application/json"
	curl http://localhost:9200/_bulk --data-binary "@./data/elasticsearch/seed-data.json" -X POST -H "Content-Type: application/x-ndjson"

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter name=test-database -a)
	-docker rm $$(docker ps -q --filter name=test-database -a)
	docker rmi test-database
	docker-compose up -d test-database

.PHONY: remove-es-data
remove-es-data:
	-docker stop $$(docker ps -q --filter name=dev-elasticsearch -a)
	-docker rm $$(docker ps -q --filter name=dev-elasticsearch -a)
	docker volume rm $$(docker volume ls --filter name=esdata -q)

