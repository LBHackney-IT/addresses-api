dotnet restore
dotnet lambda package --project-location ./Reindex --configuration release --framework netcoreapp3.1 --output-package ./bin/release/netcoreapp3.1/reindex-es-alias.zip
dotnet lambda package --project-location ./AddressesAPI --configuration release --framework netcoreapp3.1 --output-package ./bin/release/netcoreapp3.1/addresses-api.zip
