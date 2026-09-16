FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source
COPY ["Directory.Build.props", "global.json", "LedgerFlow.slnx", "./"]
COPY ["src/LedgerFlow.Api/LedgerFlow.Api.csproj", "src/LedgerFlow.Api/"]
COPY ["src/LedgerFlow.Application/LedgerFlow.Application.csproj", "src/LedgerFlow.Application/"]
COPY ["src/LedgerFlow.Domain/LedgerFlow.Domain.csproj", "src/LedgerFlow.Domain/"]
COPY ["src/LedgerFlow.Infrastructure/LedgerFlow.Infrastructure.csproj", "src/LedgerFlow.Infrastructure/"]
RUN dotnet restore src/LedgerFlow.Api/LedgerFlow.Api.csproj
COPY src ./src
RUN dotnet publish src/LedgerFlow.Api/LedgerFlow.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
RUN addgroup -S ledgerflow && adduser -S ledgerflow -G ledgerflow
COPY --from=build --chown=ledgerflow:ledgerflow /app .
USER ledgerflow
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "LedgerFlow.Api.dll"]
