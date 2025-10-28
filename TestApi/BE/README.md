# AutoApiTester (.NET 8 Web API) - Full API (no SeedData)

This package contains a full .NET 8 Web API project with:
- JWT Auth (register/login)
- CRUD controllers for Users, Workspaces, Collections, Requests, Environments, ExecutionHistories
- Real HTTP runner using HttpClient that stores results into ExecutionHistories
- Uses SQL Server (connection string pre-filled for your instance)

## Important
- **No SeedData** included. Database must already exist with schema matching the models,
  or you should run EF Core migrations yourself.
- Connection string in `appsettings.json`:
```
Server=DESKTOP-3HSH92G\KHONGVANKC;Database=AutoApiTester;User Id=sa;Password=1234567;TrustServerCertificate=True
```

## Quick start
1. unzip
2. `dotnet restore`
3. update `appsettings.json` if needed (JWT key)
4. `dotnet run`
5. Swagger UI: `https://localhost:5001/swagger` (development)

