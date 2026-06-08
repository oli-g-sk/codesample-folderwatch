# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /repo

COPY src/ ./src/

WORKDIR /repo/src

RUN dotnet publish \
    -c Release \
    -o /repo/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0

RUN mkdir -p /public

WORKDIR /app

COPY --from=build /repo/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ServerFolderWatch.Server.dll"]