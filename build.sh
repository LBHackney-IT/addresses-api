#!/bin/bash

#install zip on debian OS, since microsoft/dotnet container doesn't have zip by default
if [ -f /etc/debian_version ]
then
  apt -qq update
  apt -qq -y install zip
fi

#dotnet restore
dotnet tool install --global Amazon.Lambda.Tools --version 4.0.0


# (for CI) ensure that the newly-installed tools are on PATH
if [ -f /etc/debian_version ]
then
  export PATH="$PATH:/$(whoami)/.dotnet/tools"
fi

dotnet restore
dotnet lambda package --project-location ./Reindex --configuration release --framework net8.0 --output-package ./Reindex/bin/release/net8.0/reindex-es-alias.zip
dotnet lambda package --project-location ./AddressesAPI --configuration release --framework net8.0 --output-package ./AddressesAPI/bin/release/net8.0/addresses-api.zip
