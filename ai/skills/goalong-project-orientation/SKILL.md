# GoAlong Project Orientation

Use this skill when starting any task in this repository, especially when the request is broad or the affected area is unclear.

## Project Shape

This is `goalongapi`, an ASP.NET Core Web API targeting `.NET 9.0`.

Important files and folders:

- `Program.cs` - application bootstrap, CORS, Swagger, auth, SignalR hub mapping, service registration.
- `goalongapi.csproj` - package references and target framework.
- `Installers/` - dependency registration through `IInstallers` and `InstallServiceInAssembly`.
- `Data/DatabaseContext.cs` - main EF Core SQL Server context.
- `Data/HRDatabaseContext.cs` - HR-related EF Core context.
- `DB/DBConn.cs` - legacy static SQL helper for stored procedures, `DataTable`, and manual transactions.
- `Controllers/` - API endpoints. There are many feature-specific controllers.
- `Models/`, `Dtos/`, `Entities/` - request/response/domain models.
- `Services/`, `Interfaces/` - service classes. Some concrete services live under `Interfaces/Services`.
- `hub/` - SignalR hubs.
- `wwwroot/allfileupload/` - uploaded/static files.

## Runtime Stack

- SQL Server connection key: `ConnectionStrings:ConnectionSQLServer`.
- JWT settings section: `JwtSettings`.
- RabbitMQ settings: `RabbitMQ:Host`, `RabbitMQ:QueueName`.
- Swagger is enabled unconditionally in `Program.cs`.
- CORS policy name: `_MyAllowSpecificOrigins`.
- Static files are served through `app.UseStaticFiles()`.

## First Steps

1. Inspect the target controller/model/service before editing.
2. Check whether the feature uses EF Core or `DB.DBConn` stored procedure calls.
3. Check route style in nearby endpoints before adding new routes.
4. Check whether `[Authorize]` or `[AllowAnonymous]` is expected.
5. Check whether response shape uses `MsgReturn`, raw JSON string, typed DTO, or anonymous object.

## Local Patterns

- Controllers often use `[ApiController]` plus either `[Route("[controller]")]` or `[Route("api/[controller]")]`.
- Many legacy endpoints add route segments like `[Route("api/name")]` or `[HttpGet("[action]")]`.
- Modern endpoints use injected services and typed DTOs.
- Legacy endpoints frequently build stored procedure strings and return serialized `DataTable`.
- Services named `*Service` are auto-registered by Autofac as implemented interfaces in `Program.cs`.

## Cautions

- Do not assume all controllers follow one route convention.
- Do not rename existing route actions casually; frontend clients may depend on exact paths.
- Do not move existing models between `Models`, `Dtos`, and `Entities` unless the task requires it.
- Avoid touching `wwwroot/allfileupload` contents unless the task is specifically about uploads.
- The `.gitignore` currently contains a malformed glob line, so `rg` may print an ignore parse warning. Use targeted file commands if needed.

