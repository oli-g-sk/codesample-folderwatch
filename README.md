## Getting started

## Server app

- Use `dotnet run` in `src/ServerFolderWatch.Server` to start the server at http://localhost:5000/
- Use `docker compose up` to start the server in a container exposed at http://localhost:8080/
- Both point to the `shared` folder here; use it to test the app
- *Snapshots are taken on startup*; restart is required 
- *Blazor UI* available at http://localhost:8080/browse

## API endpoints

- GET `http://localhost:8080/api/browse`
- GET `http://localhost:8080/api/browse?folder=path/to/folder`

- GET `http://localhost:8080/api/diff`
- GET `http://localhost:8080/api/diff?folder=path/to/folder`

## CLI tool