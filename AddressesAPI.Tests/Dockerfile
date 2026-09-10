FROM mcr.microsoft.com/dotnet/sdk:10.0

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

# #region agent log
CMD sh -c 'mkdir -p /app/.cursor; echo "{\"sessionId\":\"f2761d\",\"hypothesisId\":\"A\",\"location\":\"Dockerfile:CMD\",\"message\":\"dotnet test starting\",\"data\":{\"pwd\":\"$(pwd)\",\"sln\":\"$(ls *.sln 2>/dev/null)\"},\"timestamp\":$(date +%s000)}" >> /app/.cursor/debug-f2761d.log; dotnet test; ec=$?; echo "{\"sessionId\":\"f2761d\",\"hypothesisId\":\"A\",\"location\":\"Dockerfile:CMD\",\"message\":\"dotnet test finished\",\"data\":{\"exitCode\":$ec},\"timestamp\":$(date +%s000)}" >> /app/.cursor/debug-f2761d.log; exit $ec'
# #endregion
