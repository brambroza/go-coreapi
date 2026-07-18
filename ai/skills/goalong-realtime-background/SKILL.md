# GoAlong Realtime And Background Skill

Use this skill when changing SignalR hubs, ticket comments, chat, notifications, RabbitMQ, or hosted background services.

## SignalR

SignalR is enabled in `Program.cs`:

```csharp
builder.Services.AddSignalR();
```

Mapped hubs:

- `/notificationhub` -> `NotificationHub`
- `/tickettaskreplyhub` -> `TicketTaskReplyHub`
- `/ticketcommenthub` -> `TicketCommentHub`
- `/chathub` -> `ChatHub`
- `/sessionhub` -> `SessionHub`

Hub files are in `hub/`.

When adding hub behavior:

- Check the matching controller and frontend event names if available.
- Keep hub route paths stable.
- Prefer typed event names only if existing clients support them.
- Use `IHubContext<T>` in controllers/services when sending from outside a hub.

## Ticket Comments

`Controllers/CommentTicketController.cs` builds ticket comment conversations from multiple stored procedures and uses `IHubContext<TicketCommentHub>`.

When changing ticket comments:

- Preserve response wrapper names such as `Conversations` and `Conversation`.
- Preserve message, attachment, and participant field names expected by clients.
- Check all stored procedure calls with `ticket_` prefix.
- Be careful with transaction blocks for message and attachment saves.

## RabbitMQ Log Processing

Producer:

- `Services/RabbitMQService.cs`
- Method: `SendLog(LogRequest log)`

Consumer/background service:

- `Services/LogProcessorService.cs`
- Registered in `Program.cs` with `AddHostedService<LogProcessorService>()`

Config keys:

- `RabbitMQ:Host`
- `RabbitMQ:QueueName`

The background service consumes messages, deserializes `LogRequest`, writes `LogSystemClick`, then acknowledges the message.

## Background Service Cautions

When changing `LogProcessorService`:

- Use a DI scope before resolving `DatabaseContext`.
- Await async database calls.
- Acknowledge messages only after successful persistence.
- Decide explicitly whether failures should requeue or dead-letter.
- Avoid throwing from startup unless the API should fail when RabbitMQ is unavailable.

Current code throws if RabbitMQ connection fails in constructor. Changing that behavior affects service startup semantics.

## Checklist

- Hub route paths remain stable.
- Client event names and response wrapper names are preserved.
- RabbitMQ config keys are unchanged.
- Background service uses scoped services correctly.
- Message acknowledgements happen after successful processing.
- Failures are logged and handled intentionally.

