# Server Folder Diff

A pair of .NET Core applications which allow on-demand detection of folder content changes.

The **ASP.NET server app** sets up folder monitoring within a predefined file system subtree
and provides REST endpoints and a web UI to compare the current file system state
against the last snapshot.

An accompanying **CLI tool** can be used to perform the same operations from within the
command line, at any location within the file system.

## Package sources

The WPF desktop app currently depends on `Olivercode.WPFastr`, which is published to
GitHub Packages until it is available on NuGet.org. Add this package source before
restoring or building the solution:

```powershell
dotnet nuget add source https://nuget.pkg.github.com/oli-g-sk/index.json -n github
```

## Server app

- Either use `dotnet run` in the `ServerFolderWatch.Server` folder
- Or start in a container exposed at http://localhost:8080/ using `docker compose up`

### Notes

- The `shared` folder in this repo is **mounted as the root for change tracking**; you can use it to test the app
- **Snapshots are taken on startup**; displayed _diffs_ compare the last snapshot with the current file system state
- The root path to be monitored is defined in `appsettings.json` in the Server project
- For deployment, it can be overridden by setting the `App__RootPublicPath` environment variable (as does `compose.yaml`)

## Web UI
- When booted up with Docker Compose, **Blazor UI** is available at http://localhost:8080/browse
- Please note that *version is only shown* if it's *greater than 1* and modifying a file while the app is running will require a restart

## CLI tool

- Build the `ServerFolderWatch.CLI` in _Debug_ or _Release_ mode
- **Go to the output folder** and **add it to your PATH** so you can easily execute it from anywhere 

```powershell
cd .\build\Debug\
[Environment]::SetEnvironmentVariable(
    "PATH",
    [Environment]::GetEnvironmentVariable("PATH", "User") + ";$PWD",
    "User"
)
```

- Restart your terminal
- From any folder, type `fdif` to see any changes in the current folder
- Type `fdif commit` to either **start tracking** the folder or **commit changes**
- Use `fdif commitr` same as above, but to run recursively on all subfolders

## API endpoints

### Browse folder contents (flat)

```http
GET /api/browse
GET /api/browse?folder=path/to/folder
```

```json
{
  "contents": [
    {
      "name": "folder-name",
      "type": "Directory",
      "version": null
    },
    {
      "name": "file-name.txt",
      "type": "File",
      "version": 1
    },
  ]
}
```

### Show diff compared to last snapshot

```http
GET /api/diff
GET /api/diff?folder=path/to/folder
```

```json
{
  "lastAnalyzed": "2026-06-11T07:54:41.3752154+02:00",
  "changes": [
    {
      "name": "some-folder",
      "type": "Directory",
      "diffOperation": "Unchanged",
      "version": null
    },
    {
      "name": "file.txt",
      "type": "File",
      "diffOperation": "Added",
      "version": 1
    },
    {
      "name": "hello.world",
      "type": "File",
      "diffOperation": "Modified",
      "version": 2
    },
    {
      "name": "justcreated",
      "type": "File",
      "diffOperation": "Removed",
      "version": 3
    }
  ],
  "path": "new folder/"
}
```

## TODO

### Features

- [ ] More robust way of detecting file content changes (size, hash)
- [ ] Loading state in the web UI
- [ ] A /commit API endpoint
- [ ] Ability to detect _rename_
- [ ] Show file sizes in UI
- [ ] Show a status bar

### Code quality

- [ ] Use scoped services where possible
- [ ] Composition instead of inheritance for _snapshot service_
- [ ] Don't allow comparison of different folders


### Test coverage

- [ ] Snapshot date is always saved
- [ ] File version gets incremented
- [ ] Browse service only accepts folder paths
- [ ] Browse service always only lists _names_
