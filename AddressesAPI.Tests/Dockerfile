FROM mcr.microsoft.com/dotnet/sdk:8.0

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./AddressesAPI.sln ./
COPY ./AddressesAPI/AddressesAPI.csproj ./AddressesAPI/
COPY ./AddressesAPI.Tests/AddressesAPI.Tests.csproj ./AddressesAPI.Tests/

RUN dotnet restore ./AddressesAPI/AddressesAPI.csproj
RUN dotnet restore ./AddressesAPI.Tests/AddressesAPI.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out AddressesAPI.Tests/AddressesAPI.Tests.csproj

CMD dotnet test
