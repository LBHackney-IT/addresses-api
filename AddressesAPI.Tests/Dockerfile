FROM mcr.microsoft.com/dotnet/sdk:10.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./AddressesAPI.sln ./
COPY ./global.json ./
COPY ./AddressesAPI/AddressesAPI.csproj ./AddressesAPI/
COPY ./AddressesAPI.Tests/AddressesAPI.Tests.csproj ./AddressesAPI.Tests/
COPY ./Reindex/Reindex.csproj ./Reindex/
COPY ./ReindexTests/ReindexTests.csproj ./ReindexTests/

RUN dotnet restore AddressesAPI.sln

# Copy everything else and build
COPY . .

RUN dotnet build -c Debug AddressesAPI.sln

CMD dotnet test AddressesAPI.sln --no-build -c Debug -m:1
