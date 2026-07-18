# GoAlong Database And SQL Skill

Use this skill when touching EF Core contexts, models mapped to SQL Server, stored procedure calls, or `DB.DBConn`.

## Database Access Modes

This repo uses two database access styles:

1. EF Core through `DatabaseContext` and `HrDbContext`.
2. Legacy raw SQL/stored procedure calls through static `DB.DBConn`.

Choose based on the existing feature. Do not rewrite a legacy stored-procedure controller to EF Core unless the task explicitly asks for that migration.

## EF Core

Main context:

```csharp
goalongapi.Data.DatabaseContext
```

HR context:

```csharp
goalongapi.Data.HrDbContext
```

Registration is in `Installers/DatabaseInstaller.cs` and `Program.cs`.

When adding EF models:

- Add the `DbSet` to the correct context.
- Configure table name, keys, max lengths, required fields, indexes, relationships, and delete behavior in `OnModelCreating` when needed.
- Follow existing schema names such as `msb` when adjacent models use schemas.
- Use `UseSqlOutputClause(false)` for SQL Server tables with triggers or where nearby mappings already require it.

## Legacy DBConn

`DB/DBConn.cs` stores static `SqlConnection`, `SqlCommand`, and `SqlTransaction` fields. This is legacy and risky under concurrent requests.

When maintaining existing code:

- Keep changes narrow.
- Prefer adding parameterized `SqlCommand` code if introducing new SQL.
- Avoid adding more string-concatenated SQL when user input is involved.
- Close/dispose connections and transactions in all paths.
- Avoid sharing mutable static command state outside the existing helper.

Existing code often does this:

```csharp
string cmd = "exec dbo.proc @CmpId='" + cmpid + "'";
DataTable dt = DB.DBConn.GetDataTable(cmd);
```

For new code, prefer parameterized SQL:

```csharp
using var conn = new SqlConnection(connectionString);
using var cmd = new SqlCommand("dbo.proc", conn);
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("@CmpId", cmpid);
```

## Stored Procedure Endpoints

Many controllers call procedures named with feature prefixes, for example:

- `ticket_getCommentConversation`
- `ticket_setComment`
- `get_versioninfo`
- `set_versioninfo`
- `getLogSystemClick`

Before changing procedure calls:

1. Search all usages of the procedure name.
2. Preserve exact parameter names.
3. Preserve output shape expected by frontend mapping code.
4. Check whether the endpoint uses transactions.

## DataTable Mapping

When mapping `DataTable` rows to models:

- Guard nullable fields when new code can receive missing values.
- Use `TryParse` for new parsing logic.
- Keep date formatting stable for stored procedures.
- Avoid `DataTable.Select` filters with unescaped user-provided values in new code.

## Transactions

Existing transaction pattern:

```csharp
DB.DBConn.SqlConnectionOpen();
DB.DBConn.Cmd = DB.DBConn.Cnn.CreateCommand();
DB.DBConn.Tran = DB.DBConn.Cnn.BeginTransaction();
```

When editing transaction code:

- Commit only after all commands succeed.
- Roll back on exceptions and failed commands.
- Dispose transaction and command/connection.
- Do not return before cleanup.

## Security Notes

Raw string SQL with user input is SQL injection-prone. When adding new database code, use parameters. When fixing existing code, prioritize parameterization if the endpoint accepts query/body values from clients.

Do not commit real credentials or generated token files. App settings and local token files may already contain sensitive values, so avoid echoing them in logs or docs.

