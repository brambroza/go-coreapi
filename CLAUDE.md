# CLAUDE.md — goalongapi (GoAlong Core API)

คำสั่งสำหรับ Claude Code เมื่อทำงานใน repo นี้ ไฟล์นี้มีลำดับสูงกว่า default behavior
สำหรับภาพรวมเชิงเอกสาร (architecture diagram, config table, integration) อ่าน [README.md](README.md)

---

## 1. ตัวตนของโปรเจค

| หัวข้อ | ค่า |
| --- | --- |
| ชื่อ assembly / namespace | `goalongapi` |
| Runtime | .NET 9.0 (ASP.NET Core Web API) — SDK pin 9.0.101 (`global.json`) |
| Database | SQL Server — EF Core 9 + ADO.NET (`DbConnectionFactory`) + Stored Procedure |
| DI | Autofac auto-register ทุก class ที่ชื่อลงท้าย `Service` |
| Realtime | SignalR 6 hubs |
| Queue | RabbitMQ (`log_queue`) |
| Repo | https://github.com/brambroza/go-coreapi — branch หลัก `main` |
| Local URL | https://localhost:7046 · http://localhost:5052 · Swagger `/swagger` |
| Container | ฟัง 6600, map ออก 7046 |

Consumer ของ API นี้: `go-crm-24v4` (Next.js CRM), `NIS-OnsiteService` (React Native),
`go-chat-api` (Node.js realtime bridge) — **การเปลี่ยน route หรือ response shape กระทบ client ทั้งสามตัว**

---

## 2. Module map — ใช้หาไฟล์ก่อนเริ่มงาน

| Domain | ที่อยู่หลัก |
| --- | --- |
| CRM / Sale | `Controllers/CrmController.cs`, `LeadsController`, `CustomerController`, `QuatationController`, `QuaHController`, `SaleOrderController`, `CRMKanbanController` |
| Service Ticket / Onsite | `Controllers/ServiceTicketsController.cs`, `NisController.cs`, `NisPersonalController`, `NisPushController`, `ProblemReceiveController`, `Dtos/nis/`, `Models/Nis/` |
| Inventory / WMS | `Controllers/Inven*.cs`, `invenRtsController`, `BomController`, `Controllers/Master/WarehouseController.cs` |
| HRM / Attendance | `Controllers/HRM/`, `Dtos/hrm/`, `Data/HRDatabaseContext.cs` (`HrDbContext`) |
| Accounting | `Controllers/AccountSystem/` (AR, AP, Billing, Credit, Cost) |
| Master data | `Controllers/Master/`, `ListDataController`, `DocNoController` |
| Auth / Permission | `AuthController`, `AccountController`, `CheckLoginController`, `UserPermisstionController`, `Controllers/securitySystem/RoleController.cs`, `Installers/JwtInstaller.cs` |
| Dashboard / Report | `Controllers/Dashboard/`, `DashController`, `ReportTemplatesController`, `RevenueMobileController` |
| Integration | `Helpers/GoogleOAuth*.cs`, `Controllers/Social/`, `HookLineController`, `LineNotiController`, `MailController`, `Controllers/Email/` |
| Realtime / Background | `hub/`, `Services/` (`LogProcessorService`, `NisOverduePushService`, `ExpoPushService`, `NisRealtimeNotifyService`, `NisCrmNotifyService`, `RabbitMQService`) |

Controller ที่ใหญ่และเสี่ยงสุด (แก้ต้องระวัง regression):
`ProjectController.cs` (~4,000 บรรทัด), `DocNoController.cs` (~3,700),
`NisController.cs` (~2,600), `CrmController.cs` (~2,350), `ServiceTicketsController.cs` (~2,280)

---

## 3. กฎการเขียนโค้ดใน repo นี้

### 3.1 Data access — มี 2 pattern ห้ามผสมมั่ว

1. **EF Core** (`DatabaseContext`, `HrDbContext`) — ใช้กับงาน entity-based, CRUD ปกติ, code ใหม่
2. **ADO.NET / Stored Procedure** — `DbConnectionFactory` สำหรับ code ใหม่ที่ต้องเรียก SP,
   และ `DB/DBConn.cs` (static legacy helper) สำหรับ endpoint เก่า

กฎ:
- โค้ดใหม่ให้ใช้ EF Core หรือ `DbConnectionFactory` — **ห้ามเขียน `DBConn` static เพิ่ม**
  (`DBConn` เก็บ connection/transaction ไว้ใน static field ซึ่งไม่ thread-safe)
- แก้ endpoint เก่าที่ใช้ `DBConn` อยู่แล้ว ให้แก้ในรูปแบบเดิม อย่า refactor ทั้งไฟล์โดยไม่มี CR
- ทุก query ที่รับค่าจากผู้ใช้ต้องเป็น parameterized (`SqlParameter`) หรือ SP — **ห้าม string concat**

### 3.2 Migration

- Migration จริงของ repo นี้คือ **ไฟล์ SQL เขียนมือ** ใน `Database/Migrations/`
  ตั้งชื่อ `YYYYMMDD_ShortPascalCaseName.sql` (เช่น `20260804_AddNisTicketUpdatedBy.sql`)
- ไม่มี EF Core migration snapshot ใน repo — อย่าสร้าง `dotnet ef migrations add` โดยไม่ถามก่อน
- Script ต้อง **idempotent** (`IF NOT EXISTS ... ALTER TABLE`) เพราะรันมือบนหลาย environment
- **ห้ามรัน migration บน production หรือ database ใด ๆ โดยไม่แจ้งและได้ approval ก่อน**
- เมื่อสร้าง migration ให้บอกผู้ใช้เสมอว่ายังไม่รัน และต้องรันที่ไหนบ้าง

### 3.3 Controller

- `[ApiController]` + route แบบใดแบบหนึ่ง: `[Route("[controller]")]`, `[Route("api/[controller]")]`,
  หรือ explicit `[Route("api/ชื่อเฉพาะ")]` — **ดู controller ข้างเคียงก่อนตั้ง route ใหม่**
- ห้ามเปลี่ยนชื่อ route/action ที่มีอยู่ ถ้าไม่ได้แก้ client ทั้งสามตัวพร้อมกัน
- รับ input เป็น DTO จาก `Dtos/` ไม่รับ entity ตรง ๆ ในของใหม่
- Response shape มีหลายแบบในระบบ (`MsgReturn`, raw JSON string, typed DTO, anonymous object) —
  ใช้แบบเดียวกับ method ข้างเคียงในไฟล์นั้น
- ตรวจก่อนเสมอว่า endpoint นั้นคาด `[Authorize]` หรือ `[AllowAnonymous]`
  (`ServiceTicketsController` และ `NisController` ปัจจุบัน **ไม่มี** `[Authorize]` — เป็นของเดิม อย่าเพิ่มเองโดยไม่ถาม)
- ทุก public endpoint ต้อง validate input และคิดเรื่อง SQL injection / XSS

### 3.4 Service

- class ที่ชื่อลงท้าย `Service` จะถูก Autofac auto-register เป็น implemented interfaces
- `IHostedService` ถูก exclude ออกจาก auto-register แล้ว (`Program.cs`) — register hosted service
  ด้วย `AddHostedService` อย่างเดียว ห้าม register ซ้ำ ไม่งั้นได้ background worker ซ้อน
- Service ใหม่วางที่ `Services/` (infrastructure/background) หรือ `Interfaces/Services/` (domain service ตามของเดิม)

### 3.5 ทั่วไป

- ทุก public method / controller action ต้องมี XML doc comment (`/// <summary>`) เป็นอย่างน้อย
- เวลาในระบบ NIS ใช้เวลาไทย — ดู helper `BangkokNow()` ใน `NisController.cs` ก่อนใช้ `DateTime.Now`
- ห้าม hardcode secret / connection string / API key ใน source
- Config ใหม่ให้เพิ่มใน `appsettings.json` แล้วอ่านผ่าน `IConfiguration` เท่านั้น
- CORS origin เพิ่มที่ `Program.cs` จุดเดียว

---

## 4. คำสั่งที่ใช้บ่อย

```bash
dotnet restore
dotnet build                      # ต้องผ่านก่อนส่งงานทุกครั้ง
dotnet run --launch-profile https # https://localhost:7046/swagger
docker-compose up -d --build      # api + sqlserver + rabbitmq
```

ไม่มี test project ใน repo ปัจจุบัน — verification หลักคือ `dotnet build` + ยิง endpoint ผ่าน Swagger
ถ้าเพิ่ม test ให้เสนอสร้าง project `goalongapi.Tests` แยก และถามก่อน

---

## 5. Workflow — สั่งงานจาก Claude Code

| ต้องการ | ใช้ |
| --- | --- |
| วางแผน sprint / ตัด task | `/sprint-plan` |
| ทำ feature / task ตาม spec | `/task-feature` |
| แก้บั๊ก | `/fixbug` |
| เพิ่มหรือแก้ endpoint | `/api-endpoint` |
| แก้ schema / เขียน migration | `/db-migration` |
| ตรวจก่อนส่งงาน | `/verify` |

Agent เฉพาะทางของ repo นี้อยู่ใน [.claude/agents/](.claude/agents/)
ตารางแบ่งบทบาทและกฎการ escalate อยู่ใน [.claude/ROLES.md](.claude/ROLES.md)
Skill เดิมที่เขียนไว้ (อ้างอิงเชิงเทคนิคละเอียด) อยู่ใน [ai/skills/](ai/skills/) — ยังใช้ได้ ไม่ต้องลบ

---

## 6. Git

- Conventional Commits: `feat|fix|docs|refactor|test|chore` + scope ตาม module
  เช่น `feat(nis): add ticket reschedule endpoint`, `fix(inven): block issue over onhand`
- Branch: `feature/<ชื่อ>`, `fix/<ชื่อ>`, `chore/<ชื่อ>` — แตกจาก `main`
- **ห้าม commit หรือ push โดยไม่ได้รับคำสั่ง** · ห้าม force push บน `main`
- ห้าม commit `appsettings*.json` ที่มีค่า secret จริง, `client_secret.json`, `token.json`, `config/`

---

## 7. ข้อห้ามเด็ดขาด

- ห้ามลบไฟล์โดยไม่ขออนุญาต · ห้าม `rm -rf`
- ห้ามรัน migration หรือแตะ database จริงโดยไม่แจ้ง
- ห้าม deploy production โดยไม่ได้รับ approval
- ห้ามแก้ `.env`, `docker-compose.yml` ส่วน production, `.github/workflows/` โดยไม่แจ้ง
- ห้ามเปิดเผย credential ใน output หรือ log
- ห้าม refactor ข้ามไฟล์นอกขอบเขต task ที่สั่ง — ต้องมี Change Request

---

## 8. หนี้ทางเทคนิคที่รู้อยู่แล้ว (อย่าเพิ่งไปแก้ ถ้าไม่ได้สั่ง)

1. `appsettings.json` / `appsettings.Development.json` ถูก commit พร้อม connection string,
   JWT key และ AES key ของจริง — ควรย้ายไป User Secrets / ENV และหมุนค่าที่รั่ว
2. `DB/DBConn.cs` ใช้ static `SqlConnection` / `SqlTransaction` ร่วมกันทุก request — ไม่ thread-safe
3. Controller หลายตัวยาวเกิน 2,000 บรรทัด ควรแตก service layer
4. ไม่มี automated test เลย
5. Google OAuth client secret เก่ายังอยู่ในคอมเมนต์ใน `Program.cs`
6. `.gitignore` มี glob บรรทัดหนึ่งผิดรูปแบบ ทำให้ `rg` เตือน — ใช้คำสั่งค้นแบบระบุ path ถ้าเจอ
