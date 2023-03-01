dotnet restore
dotnet lambda package --project-location ./Reindex --configuration release --framework net6.0 --output-package ./bin/release/net6.0/reindex-es-alias.zip
dotnet lambda package --project-location ./AddressesAPI --configuration release --framework net6.0 --output-package ./bin/release/net6.0/addresses-api.zip
