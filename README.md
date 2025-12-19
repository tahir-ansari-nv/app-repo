# app-repo

## timesheet-api (ASP.NET Core 9 Web API)

- Description: Default ASP.NET Core 9 Web API scaffold.
- Project: [timesheet-api](timesheet-api)
- Solution: [timesheet-api.sln](timesheet-api.sln)

### Prerequisites
- .NET SDK 9 (verified via `dotnet --version` → 9.x)

### Restore and Build
```powershell
cd "C:\Users\tahir.ansari\source\repos\Application-Repo\app-repo"
dotnet restore .\timesheet-api\timesheet-api.csproj
dotnet build .\timesheet-api\timesheet-api.csproj -c Debug
```

### Run the API
```powershell
cd "C:\Users\tahir.ansari\source\repos\Application-Repo\app-repo"
dotnet run --project .\timesheet-api\timesheet-api.csproj
```

The template exposes a sample endpoint at `/weatherforecast`.

### Useful commands
- Run with no restore/build: `dotnet run --no-restore --no-build --project .\timesheet-api\timesheet-api.csproj`
- Trust dev HTTPS certificate (optional): `dotnet dev-certs https --trust`