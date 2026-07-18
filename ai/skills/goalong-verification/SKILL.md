# GoAlong Verification Skill

Use this skill before handing back code changes in this repository.

## Basic Commands

Preferred checks:

```bash
dotnet restore
dotnet build goalongapi.sln
```

If the solution file is not needed:

```bash
dotnet build goalongapi.csproj
```

There is no obvious test project in the current repo structure. If tests are added later, run the specific test project or `dotnet test`.

## Build Notes

The project targets `.NET 9.0`, so the local SDK must support .NET 9.

The app may require SQL Server and RabbitMQ config to run successfully. A build check is usually safer than starting the full API unless the task requires runtime verification.

## Runtime Checks

If running the API:

1. Confirm `appsettings.Development.json` or user secrets contain valid SQL Server settings.
2. Confirm RabbitMQ is available if `LogProcessorService` is enabled.
3. Start with `dotnet run --project goalongapi.csproj`.
4. Open Swagger at `/swagger`.

Do not expose or print secrets from appsettings or token files.

## Docker

Repo contains:

- `Dockerfile`
- `docker-compose.yml`
- `.github/workflows/docker-image.yml`

Use Docker only when the task is about container behavior or deployment. Build-only verification is enough for most code edits.

## Manual Review Checklist

- Routes match existing endpoint conventions.
- Auth attributes are intentional.
- DTO/model field names preserve frontend contract.
- SQL input is parameterized for new code.
- EF mappings are updated when new entities are added.
- Services are registered or auto-registerable.
- Async calls are awaited.
- No unrelated files, uploads, logs, or secrets were modified.

## Reporting

When returning results to the user:

- Mention files changed.
- Mention exact build/test command run.
- If verification was not run, say why.
- If build fails for pre-existing reasons, summarize the first relevant error and point to the file.

