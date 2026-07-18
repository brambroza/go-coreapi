# GoAlong API Controller Skill

Use this skill when adding, changing, or debugging API endpoints in `Controllers/`.

## Read First

Before editing an endpoint:

1. Read the target controller.
2. Read related model/DTO classes.
3. Read related service or SQL helper usage.
4. Check route attributes and response shape in neighboring methods.

## Controller Conventions

Common controller shapes in this repo:

```csharp
[ApiController]
[Route("[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
}
```

```csharp
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SystemLogController : ControllerBase
{
}
```

Route actions can be explicit:

```csharp
[Route("api/CheckLogin")]
[HttpGet]
```

or action-based:

```csharp
[HttpGet("[action]")]
```

Match the local controller style instead of normalizing the whole file.

## Dependency Injection

Prefer constructor injection when a service already exists:

- EF Core contexts: `DatabaseContext`, `HrDbContext`
- Hubs: `IHubContext<TicketCommentHub>`
- App services: interfaces such as `IProductService`
- Singleton service: `RabbitMQService`

Concrete classes ending with `Service` are auto-registered by Autofac as implemented interfaces. If a new service must be injected by interface, name the implementation `SomethingService` and implement `ISomethingService`.

## Request Binding

Use binding attributes explicitly when adding new endpoints:

- `[FromQuery]` for query strings.
- `[FromBody]` for JSON body.
- `[FromForm]` for multipart/form-data or file uploads.
- Route parameters for stable resource identifiers.

For file uploads, follow the existing `ProductsController` and upload services before creating a new file-write pattern.

## Response Patterns

Keep response style compatible with the existing endpoint family:

- Typed DTO/list for newer EF/service endpoints.
- `MsgReturn` for many save/update stored-procedure endpoints.
- Anonymous object wrappers for chat/comment endpoints, such as `{ Conversations = res }`.
- Existing legacy login endpoints may return serialized `DataTable` strings.

When adding a new endpoint, prefer typed DTOs unless adjacent endpoints clearly use the legacy style.

## Auth

Most controllers are `[Authorize]`. Use `[AllowAnonymous]` only for login, external callbacks, public hooks, or endpoints that are already public by design.

Do not remove `[Authorize]` from a controller to make one action public. Add `[AllowAnonymous]` only on that action.

## Error Handling

Use clear HTTP results:

- `NotFound()` when a specific record is absent.
- `BadRequest(...)` for invalid input or failed save.
- `Ok(...)` for reads and successful commands returning data.
- `NoContent()` for successful deletes without body.
- `StatusCode((int)HttpStatusCode.Created, ...)` or `Created...` for creates.

Do not hide data errors behind `Ok` unless the existing endpoint contract already does that.

## Implementation Checklist

- Route path matches existing client-facing convention.
- `[Authorize]` or `[AllowAnonymous]` is intentional.
- Input binding is explicit.
- Response shape matches nearby endpoints.
- Async EF calls are awaited.
- Services are registered or auto-registerable.
- Stored procedure names and parameter names match existing SQL conventions.

