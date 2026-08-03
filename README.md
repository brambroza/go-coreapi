# GoAlong Core API (`goalongapi`)

Core backend API ของระบบ GoAlong / NIS Solution — ให้บริการงาน CRM, HRM, Inventory/WMS,
Service Ticket & Onsite Service, Quotation/Sale Order และ Realtime Notification
ผ่าน REST API + SignalR

| หัวข้อ | รายละเอียด |
| --- | --- |
| Runtime | .NET 9.0 (ASP.NET Core Web API) |
| SDK ที่ pin ไว้ | 9.0.101 (`global.json`) |
| Database | SQL Server (EF Core 9 + ADO.NET ผ่าน `DbConnectionFactory`) |
| DI Container | Autofac (auto-register ทุก type ที่ลงท้ายด้วย `Service`) |
| Realtime | SignalR (6 hubs) |
| Message Queue | RabbitMQ (system log pipeline) |
| API Docs | Swagger UI — เปิดใช้งานทุก environment |
| Repository | https://github.com/brambroza/go-coreapi |

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Running with Docker](#running-with-docker)
- [SignalR Hubs](#signalr-hubs)
- [Background Services](#background-services)
- [External Integrations](#external-integrations)
- [CI/CD](#cicd)
- [Conventions](#conventions)
- [Security Notes](#security-notes)

---

## Architecture Overview

```
                 ┌────────────────────────┐
  go-crm-24v4 ──▶│                        │
  (Next.js CRM)  │                        │──▶ SQL Server (GoAlongDatabase)
                 │   GoAlong Core API     │
 NIS-OnsiteService──▶  ASP.NET Core 9     │──▶ RabbitMQ  (log_queue)
  (React Native) │   REST + SignalR       │
                 │                        │──▶ Google APIs (Calendar / Gmail / Sheets)
  go-chat-api ◀──│                        │──▶ Expo Push  (mobile notification)
  (Node.js)      └────────────────────────┘──▶ LINE Messaging API
```

- **Composition root** — [Program.cs](Program.cs) ประกอบ service ทั้งหมด และใช้ pattern
  `IInstaller` ([Installers/](Installers/)) เพื่อแยก concern ของ CORS, JWT, Swagger,
  Database และ Controller registration ออกจากกัน
- **Auto DI** — Autofac สแกน entry assembly แล้ว register ทุก class ที่ชื่อลงท้ายด้วย
  `Service` เข้ากับ interface ที่ implement โดยอัตโนมัติ ([Program.cs:120-126](Program.cs#L120-L126))
- **Data access** — ใช้ 2 แบบผสมกัน: EF Core (`HrDbContext`, `DatabaseContext`) สำหรับงาน
  entity-based และ `DbConnectionFactory` + Stored Procedure สำหรับงาน report/transaction
  ที่ต้องการ performance

---

## Project Structure

```
coreapi-new/
├── Program.cs              # Composition root: DI, CORS, middleware pipeline, hub routes
├── goalongapi.csproj       # net9.0 + package references
├── global.json             # Pin .NET SDK 9.0.101
├── Dockerfile              # Multi-stage build, non-root user, EXPOSE 6600
├── docker-compose.yml      # api + sqlserver + rabbitmq
├── Installers/             # IInstaller modules (Cors, Jwt, Swagger, Database, Controller)
├── Controllers/            # REST endpoints แยกตาม domain + โฟลเดอร์ย่อยตาม module
│   ├── HRM/  Master/  Dashboard/  Email/  Social/  AccountSystem/  securitySystem/
├── Services/               # Background & infrastructure services
├── Helpers/                # Google OAuth, AES crypto, PDF storage, repositories
├── Interfaces/             # Service contracts
├── Entities/  Models/  Dtos/   # Domain model, request/response contracts
├── Data/  DB/  Database/       # DbContext, connection factory, EF migrations
├── hub/                    # SignalR hub implementations
└── wwwroot/                # Static files & uploaded assets
```

---

## Getting Started

### Prerequisites

- [.NET SDK 9.0.101](https://dotnet.microsoft.com/download) ขึ้นไป
- SQL Server 2019+ (หรือรันผ่าน Docker)
- RabbitMQ 3.x (จำเป็นสำหรับ log pipeline)

### Local development

```bash
# 1. Restore dependencies
dotnet restore

# 2. ตั้งค่า connection string และ secret (ดูหัวข้อ Configuration)
dotnet user-secrets set "ConnectionStrings:ConnectionSQLServer" "<your-connection-string>"

# 3. Run
dotnet run --launch-profile https
```

| Endpoint | URL |
| --- | --- |
| HTTPS | https://localhost:7046 |
| HTTP | http://localhost:5052 |
| Swagger UI | https://localhost:7046/swagger |

> ใน `Development` จะ **ไม่** บังคับ HTTPS redirect เพื่อให้ mobile/simulator
> (Expo Go, iPad บน LAN) เรียก `http://<LAN-IP>:5052` ได้ตรง — production ยัง redirect ตามปกติ
> ([Program.cs:150-153](Program.cs#L150-L153))

### Database migrations

```bash
dotnet ef migrations add <MigrationName> --context HrDbContext
dotnet ef database update --context HrDbContext
```

> ⚠️ ห้ามรัน migration บน production โดยไม่แจ้งทีมก่อน

---

## Configuration

ค่า config อ่านจาก `appsettings.json` → `appsettings.{Environment}.json` → User Secrets →
Environment Variables (ลำดับหลังทับลำดับก่อน) สำหรับ container ให้ใช้ ENV แบบ
`Section__Key` เช่น `RabbitMQ__HostName`

| Key | คำอธิบาย |
| --- | --- |
| `ConnectionStrings:ConnectionSQLServer` | SQL Server connection string หลัก |
| `JwtSettings:Key` / `Issuer` / `Audience` / `Expire` | ค่า JWT (`Expire` เป็นนาที) |
| `FileSizeLimit` | ขนาดไฟล์อัปโหลดสูงสุด (bytes) |
| `RabbitMQ:Host` / `Port` / `UserName` / `Password` / `QueueName` | ปลายทาง log queue |
| `EmailCrypto:KeyBase64` | AES key (base64) สำหรับเข้ารหัส credential ของ email setting |
| `GoogleOAuth:RedirectUri` | OAuth callback ของ Google Calendar / Gmail |
| `NisOnsite:AttachReportPdf` | เปิด/ปิดการแนบ Service Report PDF |
| `NisOnsite:ReportPdfDir` | โฟลเดอร์เก็บไฟล์ PDF (ว่าง = เก็บเป็น blob ใน DB) |
| `NisOnsite:MaxReportPdfBytes` | ขนาด PDF สูงสุด (default 8 MB) |
| `NisOnsite:MaxRequestBodyBytes` | Kestrel body limit — กัน base64 PDF ใหญ่เกินให้ตอบ 413 แทน connection reset (default 32 MB) |
| `NisRealtime:ChatApiBaseUrl` | Base URL ของ go-chat-api สำหรับ bridge event `nis:notify` |
| `NisRealtime:InternalSecret` | Shared secret ระหว่าง core API ↔ chat API |

### CORS

Allowed origins ถูก whitelist ไว้ใน [Program.cs:26-40](Program.cs#L26-L40) และเปิด
`AllowCredentials()` เพื่อรองรับ SignalR — เพิ่ม origin ใหม่ต้องแก้ที่จุดนี้จุดเดียว

---

## Running with Docker

```bash
# build + run ทั้ง stack (api + sqlserver + rabbitmq)
docker-compose up -d --build

# build image เดี่ยว
docker build -t goalongwebapi .
docker run -p 7046:6600 goalongwebapi
```

- Container ฟังที่พอร์ต **6600** และ map ออกมาที่ **7046**
- Dockerfile ใช้ multi-stage build และรันด้วย non-root user (`uid 5678`)
- ตั้งค่า secret ผ่าน environment variable เท่านั้น — ห้าม bake ลง image

---

## SignalR Hubs

| Hub | Route | การใช้งาน |
| --- | --- | --- |
| `NotificationHub` | `/notificationhub` | Notification กลางของระบบ |
| `TicketTaskReplyHub` | `/tickettaskreplyhub` | Reply ของ task ใน service ticket |
| `TicketCommentHub` | `/ticketcommenthub` | Comment ใน ticket |
| `ChatHub` | `/chathub` | Chat ระหว่างผู้ใช้ |
| `SessionHub` | `/sessionhub` | Session / presence tracking |
| `DispatchKanbanHub` | `/dispatchkanbanhub` | อัปเดต Kanban board ของงาน dispatch |

---

## Background Services

| Service | หน้าที่ |
| --- | --- |
| `LogProcessorService` | `IHostedService` — consume `log_queue` แล้วเขียน system log ลง DB |
| `NisOverduePushService` | ตรวจตั๋วเกินกำหนดทุก 15 นาที และส่ง push ไม่เกินวันละครั้งต่อตั๋ว |
| `RabbitMQService` | Singleton publisher สำหรับส่ง log เข้า queue |
| `ExpoPushService` | ส่ง push notification ไปยัง Expo (NIS Onsite mobile app) |
| `NisRealtimeNotifyService` | Best-effort POST ไป go-chat-api เพื่อ emit `nis:notify` (foreground refresh) |

---

## External Integrations

- **Google Calendar / Gmail** — ใช้ OAuth flow (`GoogleOAuthCalendarService`,
  `GoogleOAuthMailService`) พร้อม mapping repository สำหรับ sync event
- **Expo Push** — แจ้งเตือน background/killed state ของแอป NIS Onsite
- **go-chat-api (Node.js)** — realtime bridge สำหรับ foreground refresh
- **LINE Messaging API** — webhook และ LIFF (`HookLineController`, `LineNotiController`)
- **RabbitMQ** — asynchronous system log pipeline

---

## CI/CD

[`.github/workflows`](.github/workflows/) — เมื่อ push ขึ้น `main` จะ build Docker image
และ push ไปยัง Docker Hub (`nohservdoc/go-crmapi24`) โดยใช้ secrets
`DOCKER_USERNAME` / `DOCKER_PASSWORD`

---

## Conventions

- **Commit message** — Conventional Commits: `feat|fix|docs|refactor|test|chore`
  เช่น `feat(nis): add overdue push watcher`
- **Branch** — ห้าม force push บน `main`; deploy production ต้องได้ approval
- **Controller** — 1 domain ต่อ 1 controller, ใช้ DTO ใน `Dtos/` แทนการรับ entity ตรง ๆ
- **Service** — ตั้งชื่อลงท้ายด้วย `Service` เพื่อให้ Autofac auto-register ได้
- **Input validation** — ทุก public endpoint ต้อง validate input และใช้ parameterized
  query / stored procedure เสมอ

---

## Security Notes

> ⚠️ ปัจจุบัน `appsettings.json` และ `appsettings.Development.json` ยังถูก commit
> ลง repository พร้อมค่า connection string, JWT key และ AES key ของจริง
> ควรย้ายไปใช้ User Secrets (local) และ Environment Variable / secret manager (production)
> แล้วเปลี่ยนค่าที่เคยรั่วทั้งหมด

- ห้าม hardcode credential ใน source code
- `client_secret.json`, `config/`, `token.json` ถูก ignore ไว้แล้ว — อย่า commit
- ตรวจ SQL injection / XSS ทุกครั้งที่เพิ่ม endpoint ใหม่
