FROM mcr.microsoft.com/dotnet/sdk:10.0.9-noble AS build
WORKDIR /src

COPY BingoCompany.Domain/BingoCompany.Domain.csproj BingoCompany.Domain/
COPY BingoCompany.Application/BingoCompany.Application.csproj BingoCompany.Application/
COPY BingoCompany.Infrastructure/BingoCompany.Infrastructure.csproj BingoCompany.Infrastructure/
COPY BingoCompany.Api/BingoCompany.Api.csproj BingoCompany.Api/

RUN dotnet restore BingoCompany.Api/BingoCompany.Api.csproj

COPY BingoCompany.Domain/ BingoCompany.Domain/
COPY BingoCompany.Application/ BingoCompany.Application/
COPY BingoCompany.Infrastructure/ BingoCompany.Infrastructure/
COPY BingoCompany.Api/ BingoCompany.Api/

RUN dotnet publish BingoCompany.Api/BingoCompany.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.9-noble AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

USER $APP_UID

HEALTHCHECK --interval=10s --timeout=3s --start-period=20s --retries=6 CMD curl --fail --silent http://127.0.0.1:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "BingoCompany.Api.dll"]
