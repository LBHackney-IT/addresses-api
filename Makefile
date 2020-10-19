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
	docker-compose up test-database & docker-compose build addresses-api-test && docker-compose up addresses-api-test

.PHONY: migrate-dev-database
migrate-dev-database:
	-dotnet tool install -g dotnet-ef
	cd AddressesAPI && CONNECTION_STRING="Host=127.0.0.1;Port=5433;Database=devdb;Username=postgres;Password=mypassword" dotnet ef database update

.PHONY: seed-dev-database
seed-dev-database:
	docker exec $$(docker ps -q --filter ancestor=dev-database) psql -d devdb -U postgres -f /var/seed-dev-data.sql


.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=test-database -a)
	-docker rm $$(docker ps -q --filter ancestor=test-database -a)
	docker rmi test-database
	docker-compose up -d test-database
