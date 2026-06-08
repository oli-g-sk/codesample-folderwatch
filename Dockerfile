# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY src/ ./src/

RUN dotnet publish src/ServerFolderWatch.Server/ServerFolderWatch.Server.csproj \
    -c Release \
    -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ServerFolderWatch.Server.dll"]