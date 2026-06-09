# Server Folder Diff

## Starting the app

- Start the server in a container, exposed at http://localhost:8080/: `docker compose up`
- The `shared` folder is **mounted as the root for change tracking**; use it to test the app
- **Snapshots are taken on startup**; displayed _diffs_ compare the last snapshot with the current file system state

## Web UI
- When booted up with Docker Compose, **Blazor UI** is available at http://localhost:8080/browse

## API endpoints

- GET `/api/browse`
- GET `/api/browse?folder=path/to/folder`

- GET `/api/diff`
- GET `/api/diff?folder=path/to/folder`