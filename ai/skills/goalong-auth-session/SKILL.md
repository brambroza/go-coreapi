# GoAlong Auth And Session Skill

Use this skill when changing login, JWT, authorization, account/session models, or external account behavior.

## Auth Setup

JWT is configured in `Installers/JwtInstaller.cs`.

Settings section:

```json
{
  "JwtSettings": {
    "Key": "...",
    "Issuer": "...",
    "Audience": "...",
    "Expire": "..."
  }
}
```

`Program.cs` calls:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Most controllers require `[Authorize]`.

## Login Area

Important files:

- `Controllers/CheckLoginController.cs`
- `Controllers/AccountExternalController.cs`
- `Interfaces/AccountService.cs`
- `Interfaces/IAccountService.cs`
- `Entities/Account.cs`
- `Data/DatabaseContext.cs`

Existing login endpoints call stored procedures such as `dbo.CheckLogin` and may return serialized `DataTable` strings. Preserve existing contract unless the user asks for a breaking API change.

## Session Model

`AccountSession` is configured in `DatabaseContext.OnModelCreating`:

- Table: `AccountSessions`
- Key: `SessionId`
- `DeviceId` required, max length 64
- `DeviceName` max length 128
- `UserAgent` max length 512
- `IpAddress` max length 45
- FK to `AccountID`

When changing sessions, inspect the entity/model definition and all controller usages first.

## Authorization Rules

- Keep `[Authorize]` at controller level for protected APIs.
- Use `[AllowAnonymous]` only for public login/callback/hook endpoints.
- Do not relax auth globally in `Program.cs`.
- If adding role or claim checks, verify token creation includes those claims.

## JWT Changes

When changing token validation:

- Keep issuer and audience validation aligned with clients.
- Keep `ClockSkew = TimeSpan.Zero` unless there is a clear reason.
- Avoid changing `JwtSettings` names because config binding depends on property names.
- Do not log token values.

## Security Improvements

If touching login SQL, prefer parameterized stored procedure calls. Existing code concatenates username/password/CmpId into SQL strings; do not copy that pattern into new code.

If handling passwords:

- Do not add plaintext password storage.
- Do not write passwords to logs.
- Avoid returning password fields in response DTOs.

## Checklist

- Endpoint auth attributes are intentional.
- Token issuer/audience/key are read from config.
- No secret/token/password is logged or hardcoded.
- Existing frontend response contract is preserved.
- Session changes match `DatabaseContext` mapping.

