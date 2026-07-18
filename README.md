# goalongapi — API Guide (System Index)

คู่มือ **index ทั้งระบบ** ของ `coreapi-26v1` (goalongapi, .NET / ASP.NET Core)
สำหรับให้คนใหม่เข้าใจว่า "มี endpoint กลุ่มไหนบ้าง แต่ละ controller ทำอะไร และเรียกใช้ยังไง"

> เอกสารนี้เป็น **สารบัญ (catalog)** ระดับ controller ไม่ได้ลงรายละเอียดทุก endpoint
> ต้องการรายละเอียดราย endpoint (parameter/response) → เปิด **Swagger UI** (ดูหัวข้อ [ดู Swagger](#ดู-swagger-ราย-endpoint))

- **จำนวน:** 114 controllers · ~813 endpoints
- **อัปเดตล่าสุด:** 2026-07-17

---

## 1. เริ่มต้นใช้งาน (Getting Started)

### Base URL
| Environment | Base URL |
|---|---|
| Production (ที่ frontend ใช้จริง) | `https://api.nisolution.co.th` |
| Config ฝั่ง frontend | `VITE_SERVER_URL` ใน `go-crm-26v2/src/config-global.ts` |

### Authentication — JWT Bearer
API ใช้ **JWT Bearer token** (config ที่ `appsettings.json` → `Jwt`)

| Field | Value |
|---|---|
| Scheme | `Bearer` (HTTP Authorization header) |
| Issuer | `GoAlong` |
| Audience | `https://app.goalong.co.th` |
| อายุ token | 60 นาที |

**ขั้นตอน:**
1. ขอ token โดย login → `POST /Account/Login` (ดู [Auth & Account](#2-auth--account--users))
2. แนบ token ทุก request ที่ต้อง auth:
   ```
   Authorization: Bearer <token>
   ```
3. Endpoint ที่มี `[Authorize]` (ดูคอลัมน์ **Auth** ในตาราง) จะ **401** ถ้าไม่มี token ที่ถูกต้อง

Middleware order (ใน `Program.cs`): `UseCors` → `UseAuthentication` → `UseAuthorization` → `MapControllers`

### Query param ที่พบเกือบทุก endpoint
ระบบเป็น **multi-tenant** — endpoint ส่วนใหญ่รับ 2 ตัวนี้ (ชื่ออาจต่างเคสเล็กน้อย):

| Param | ความหมาย |
|---|---|
| `cmpid` / `CmpId` | รหัสบริษัท (tenant) — **จำเป็นเกือบทุกครั้ง** |
| `user` / `userlogin` | username ผู้เรียก (ใช้ทำ audit / filter สิทธิ์) |

> ฝั่ง frontend inject 2 ตัวนี้ผ่าน `useAuthContext()` (`cmpid`, `userlogin`) แล้วส่งเป็น `params` ดู pattern ที่ `go-crm-26v2/CLAUDE.md`

---

## 2. ⚠️ กฎการ Resolve Route — อ่านก่อนเดา URL

จุดนี้ **สับสนที่สุด** เพราะ controller ในโปรเจกต์นี้ใช้ route หลายแบบปนกัน
URL จริงขึ้นกับ attribute ที่ระดับ **class** + ระดับ **method** รวมกัน:

| แบบ (class attribute) | ตัวอย่าง controller | URL จริง |
|---|---|---|
| `[Route("api/[controller]")]` | `CrmController` | `/api/Crm/{action}` |
| `[Route("[controller]")]` | `AccountController` | `/Account/{action}` |
| `[Route("ค่าคงที่")]` (literal) | `MasterController` → `province` | `/province/{action}` |
| **ไม่มี `[Route]` เลย** + method ใช้ `[HttpGet("[action]")]` | `ProjectController` | **`/{action}` ที่ root** (ไม่มี segment ชื่อ controller!) |

**`[action]` = ชื่อ method** เช่น `ProjectController.getProject()` → `GET /getProject`

> 🔴 **ข้อควรระวัง:** controller ที่ไม่มี `[Route]` (เช่น `ProjectController`, `CompanyController`, `PurchaseController`, `InvenTransController`, `RoleSetController`) endpoint จะโผล่ที่ **root** ตรง ๆ ตามชื่อ method — ไม่มี prefix ชื่อ controller เพราะฉะนั้น **ห้ามเดา** ว่ามี `/api/project/...` ให้เปิด Swagger ยืนยันชื่อ action เสมอ

**คอลัมน์ "Base path" ในตารางด้านล่าง** = path ที่ resolve แล้ว
`root:/{action}` หมายถึง endpoint อยู่ที่ root ตามชื่อ method

---

## 3. Catalog แยกตาม Domain

> **Auth**: ✅ = มี `[Authorize]` (ต้องมี token) · ⬜ = ไม่มี (เปิด public หรือคุมสิทธิ์เองใน code)
> **#** = จำนวน endpoint โดยประมาณในไฟล์นั้น

### 2. Auth / Account / Users
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `AccountController` | `/Account` | 31 | ✅* | ศูนย์กลาง account: register, **Login**, LoginGoogle, forgot/reset-password, ออก JWT |
| `AccountExternalController` | `/api/AccountExternal` | 1 | ⬜ | login/verify จากระบบภายนอก |
| `AuthController` | `root:/{action}` | 1 | ✅ | ตรวจ/ต่อ auth |
| `CheckLoginController` | `/CheckLogin` | 4 | ✅ | ตรวจสถานะ login / session |
| `ForgotController` | `/api/Forgot` | 2 | ⬜ | ลืมรหัสผ่าน (public) |
| `UserController` | `/User` | 9 | ✅ | จัดการผู้ใช้ (CRUD) |
| `UserPermisstionController` | `/{action}/{cmpid}` | 3 | ✅ | สิทธิ์ผู้ใช้ราย company |
| `ProfilesController` | `/api/TaskDaily` | 3 | ✅ | โปรไฟล์/งานประจำวัน (ชื่อ route แปลกจากชื่อ controller) |
| `UploadProfileController` | `/api/UploadProfile` | 4 | ⬜ | อัปโหลดรูปโปรไฟล์ |
| `RegisController` | `root:/{action}` | 2 | ⬜ | ลงทะเบียน |
| `RegisFromCustomerController` | `root:/{action}` | 15 | ⬜ | ลงทะเบียน/รับเรื่องจากลูกค้า (self-service) |
| `TrialController` | `/api/Trial` | 2 | ⬜ | สมัคร trial |

\* `AccountController` มี `[Authorize]` เฉพาะบาง action ส่วน login/register/forgot เป็น public

### 3. Security / Roles / Menu
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `RoleController` | `/api/Role` | 9 | ✅ | จัดการ role |
| `RoleSetController` | `root:/{action}` | 11 | ✅ | ตั้งค่า role / mapping |
| `SecurityRoleSettingController` | `/api/SecurityRoleSetting` | 5 | ⬜ | ตั้งค่าสิทธิ์เชิงความปลอดภัย |
| `MenuController` | `/Menu` | 2 | ✅ | เมนู/สิทธิ์การเข้าถึงเมนู |

### 4. CRM / Sales / Quotation
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `CrmController` | `/api/Crm` | 23 | ✅ | CRM หลัก (deal/pipeline/activity) |
| `CRMKanbanController` | `/api/CRMKanban` | 1 | ✅ | board แบบ Kanban ของ CRM |
| `LeadsController` | `/api/Leads` | 10 | ✅ | จัดการ lead |
| `ContactController` | `/Contact` | 5 | ✅ | ผู้ติดต่อ |
| `CustomerController` | `/Customer` | 10 | ✅ | ลูกค้า |
| `CompanyController` | `root:/{action}` | 23 | ✅ | ข้อมูลบริษัท/tenant |
| `OrganizationController` | `/api/Organization` | 9 | ⬜ | โครงสร้างองค์กร |
| `SalemanController` | `/api/Saleman` | 3 | ⬜ | พนักงานขาย |
| `SalemanTaskController` | `/api/SalemanTask` | 1 | ⬜ | งานของพนักงานขาย |
| `QuatationController` | `/api/Quatation` | 2 | ⬜ | ใบเสนอราคา |
| `QuaController` | `root:/{action}` | 2 | ✅ | ใบเสนอราคา (เสริม) |
| `QuaHController` | `/api/linenotisendapp` | 19 | ✅ | ใบเสนอราคา (header) + ส่ง Line noti (route ตั้งชื่อแปลก) |
| `QuoAppController` | `/api/QuoApp` | 5 | ⬜ | ใบเสนอราคาสำหรับ mobile app |
| `SaleOrderController` | `/SaleOrder` | 11 | ✅ | ใบสั่งขาย |
| `BomController` | `/salesbomRev` | 21 | ✅ | Bill of Materials (BOM) ฝั่งขาย |
| `VendorsController` | `root:/{action}` | 4 | ✅ | ผู้ขาย/vendor |

### 5. Service / Ticket / MA (แกนงาน operation — NIS ใช้ตารางร่วมกลุ่มนี้)
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `ServiceTicketsController` | `/api/ServiceTickets` | 43 | ⬜ | **ใหญ่สุด** — ticket งานบริการ (CRUD, subtask, action, assign) |
| `ServiceTicketMasterController` | `/api/ServiceTicketMaster` | 18 | ⬜ | master data ของ ticket |
| `ServiceModeController` | `/api/ServiceMode` | 5 | ⬜ | โหมด/ประเภทงานบริการ |
| `CommentTicketController` | `/CommentTicket` | 3 | ✅ | คอมเมนต์ใน ticket |
| `ReasonTicketController` | `root:/{action}` | 3 | ⬜ | เหตุผล/สถานะ ticket |
| `ProblemReceiveController` | `/ProblemReceive` | 9 | ✅ | รับแจ้งปัญหา |
| `ActionServiceEmpController` | `/api/ActionServiceEmp` | 1 | ⬜ | action ของพนักงานบริการ |
| `EmpWorkingOnsiteController` | `/api/EmpWorkingOnsite` | 5 | ✅ | บันทึกงาน onsite ของพนักงาน |
| `SelfJobController` | `/api/SelfJob` | 7 | ⬜ | งานที่พนักงานรับเอง |
| `SchedurController` | `/api/Schedur` | 5 | ⬜ | ตารางงาน/นัดหมายบริการ |
| `RouteController` | `root:/{action}` | 1 | ✅ | เส้นทางงาน onsite |
| `MaController` | `/api/Ma` | 4 | ⬜ | สัญญา MA (Maintenance Agreement) |
| `MADetailController` | `/api/MADetail` | 5 | ⬜ | รายละเอียด MA |
| `MAServiceController` | `/api/MAService` | 4 | ⬜ | งานบริการภายใต้ MA |
| `MATaskController` | `/api/MATask` | 4 | ✅ | งานย่อยของ MA |
| `WarrantyClaimsController` | `/api/WarrantyClaims` | 5 | ⬜ | เคลมประกัน/warranty |
| `RepeatEveryController` | `root:/{action}` | 3 | ✅ | งานที่ทำซ้ำเป็นรอบ (recurring) |

### 6. NIS Service Portal
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `NisController` | `/api/nis` | 23 | ✅ | NIS portal: project/ticket-generation + **Onsite Form** (อ่าน/เขียนตาราง ServiceTicket ชุดเดียวกับกลุ่ม Service) |
| `NisPersonalController` | `/api/nis/personal` | 7 | ✅ | ข้อมูลส่วนบุคคล/งานของ staff ใน NIS |
| `NisPushController` | `/api/nis/push` | 2 | ✅ | ลงทะเบียน Expo push token + ส่ง push |

### 7. Projects
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `ProjectController` | `root:/{action}` | 37 | ✅ | โปรเจกต์ (CRUD, task, resource) — **ทุก endpoint อยู่ root ตามชื่อ method** |

### 8. Inventory / Warehouse / Purchasing / Products
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `InvenAdjustController` | `/api/InvenAdjust` | 7 | ✅ | ปรับยอดสต็อก |
| `InvenDeliveryController` | `root:/{action}` | 3 | ✅ | ส่งของ/จ่ายออก |
| `InvenIssController` | `/api/InvenIss` | 4 | ✅ | เบิกจ่ายสินค้า (issue) |
| `InvenRcvController` | `root:/{action}` | 4 | ✅ | รับสินค้าเข้า (receive) |
| `InvenRetruntostockController` | `/api/InvenRetruntostock` | 3 | ✅ | คืนของเข้าสต็อก |
| `invenRtsController` | `/api/invenRts` | 4 | ✅ | Return-to-stock (ตัวเสริม) |
| `InvenTransController` | `root:/{action}` | 11 | ✅ | ธุรกรรมสต็อก (transaction log) |
| `InvenTransferWHController` | `/api/InvenTransferWH` | 12 | ✅ | โอนย้ายระหว่างคลัง |
| `WarehouseController` | `root:/{action}` | 7 | ✅ | คลังสินค้า (master) |
| `PurchaseController` | `root:/{action}` | 20 | ✅ | ใบสั่งซื้อ (PO) |
| `PurchaseRequestController` | `root:/{action}` | 7 | ✅ | ใบขอซื้อ (PR) |
| `ProductsController` | `/Products` | 6 | ✅ | สินค้า |
| `ProductListController` | `root:/{action}` | 4 | ✅ | รายการสินค้า (lookup) |
| `ProductSelectController` | `/api/ProductSelect` | 6 | ⬜ | เลือกสินค้า (dropdown) |
| `ProductTypeController` | `root:/{action}` | 8 | ✅ | ประเภทสินค้า |

### 9. HR / Attendance (โฟลเดอร์ `Controllers/HRM`)
> กลุ่มนี้ route มีรูปแบบเดียวกันชัดเจน: `/api/hr/{cmpId}/<resource>` — **`{cmpId}` อยู่ใน path** (ไม่ใช่ query)

| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `AttendanceDailyController` | `/api/hr/{cmpId}/attendance-daily` | 5 | ⬜ | สรุปเวลาเข้างานรายวัน |
| `AttendancePunchesController` | `/api/hr/{cmpId}/attendance-punches` | 5 | ⬜ | บันทึกตอกบัตร |
| `AttendanceRawLogsController` | `/api/hr/{cmpId}/attendance-raw-logs` | 6 | ⬜ | log ดิบจากเครื่องสแกน |
| `AttendanceAdjustmentsController` | `/api/hr/{cmpId}/attendance-adjustments` | 4 | ⬜ | ปรับแก้เวลาเข้างาน |
| `AttendanceRuleSetsController` | `/api/hr/{cmpId}/attendance-rulesets` | 6 | ⬜ | กฎการคิดเวลา |
| `ShiftsController` | `/api/hr/{cmpId}/shifts` | 5 | ⬜ | กะการทำงาน |
| `HolidaysController` | `/api/hr/{cmpId}/holidays` | 5 | ⬜ | วันหยุด |
| `LeaveRequestsController` | `/api/hr/{cmpId}/leave-requests` | 6 | ⬜ | ใบลา |
| `OTRequestsController` | `/api/hr/{cmpId}/ot-requests` | 6 | ⬜ | ใบขอ OT |
| `ScanTypesController` | `/api/hr/{cmpId}/scan-types` | 5 | ⬜ | ประเภทการสแกน |
| `DevicesScanController` | `/api/hr/{cmpId}/devices-scan` | 5 | ⬜ | เครื่องสแกน |
| `DeviceUsersScanController` | `/api/hr/{cmpId}/device-users-scan` | 5 | ⬜ | mapping ผู้ใช้กับเครื่องสแกน |
| `hrmprofileController` | `root:/{action}` | 2 | ✅ | โปรไฟล์พนักงาน HR |
| `EmpTransController` | `/api/EmpTrans` | 2 | ⬜ | ธุรกรรมพนักงาน (โอน/ย้าย) |

### 10. Dashboard / Reports / Logs
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `DashController` | `/api/Dash` | 1 | ⬜ | dashboard รวม |
| `DashboardSaleController` | `/getCongratulations` | 13 | ✅ | dashboard ฝ่ายขาย (route ตั้งชื่อแปลก) |
| `DashboardServiceController` | `/getTotalCase` | 6 | ✅ | dashboard ฝ่ายบริการ |
| `DataForDashServiceController` | `/api/DataForDashService` | 7 | ⬜ | ป้อนข้อมูลให้ dashboard บริการ |
| `RevenueMobileController` | `root:/{action}` | 6 | ✅ | รายได้ (สำหรับ mobile) |
| `ReportTemplatesController` | `/api/report-templates` | 5 | ⬜ | เทมเพลตรายงาน |
| `SystemLogController` | `/api/SystemLog` | 5 | ✅ | log ระบบ/audit |

### 11. Calendar / Scheduling
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `CalendarController` | `root:/{action}` + `/google-events` | 9 | ✅ | ปฏิทิน + sync Google Calendar (`/google-events/...`) ผูกกับ ticket |

### 12. Messaging / Notification / Line / Mail
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `ChatMessingController` | `root:/{action}` | 7 | ✅ | ข้อความแชทภายใน |
| `NotificationController` | `/api/Notification` | 7 | ⬜ | แจ้งเตือนในระบบ |
| `LineNotiController` | `/api/LineNoti` | 1 | ⬜ | ส่ง Line Notify |
| `HookLineController` | `/api/HookLine` | 1 | ⬜ | webhook รับจาก Line |
| `LineChatController` | `root:/{action}` | 3 | ✅ | แชทผ่าน Line |
| `LineExternalController` | `root:/{action}` | 1 | ⬜ | เชื่อม Line ภายนอก |
| `MailController` | `/api/Mail` | 5 | ✅ | ส่งอีเมล |
| `EmailController` | `/api/email` | 1 | ⬜ | ส่งอีเมล (endpoint แยก) |

### 13. Accounting / Finance (โฟลเดอร์ `Controllers/AccountSystem`)
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `AccountAPController` | `/api/AccountAP` | 15 | ✅ | เจ้าหนี้ (Accounts Payable) |
| `AccountARController` | `/api/AccountAR` | 14 | ✅ | ลูกหนี้ (Accounts Receivable) |
| `AccountARBillingController` | `/api/AccountARBilling` | 5 | ✅ | วางบิลลูกหนี้ |
| `AccountARCreditController` | `/api/AccountARCredit` | 5 | ✅ | เครดิต/ลดหนี้ลูกหนี้ |
| `AccountSystemController` | `/api/AccountSystem` | 13 | ✅ | ระบบบัญชีหลัก |
| `CostController` | `/api/Cost` | 13 | ✅ | ต้นทุน |
| `InvoiceController` | `/api/Invoice` | 9 | ⬜ | ใบแจ้งหนี้ |

### 14. Master Data / Config / Misc (โฟลเดอร์ `Controllers/Master` และอื่น ๆ)
| Controller | Base path | # | Auth | หน้าที่ |
|---|---|---|---|---|
| `MasterController` | `/province` | 17 | ✅ | master data รวม (จังหวัด ฯลฯ) |
| `JobtypeController` | `root:/{action}` | 3 | ✅ | ประเภทงาน |
| `ProblemTypeController` | `root:/{action}` | 3 | ✅ | ประเภทปัญหา |
| `UnitsController` | `root:/{action}` | 3 | ✅ | หน่วยนับ |
| `ListDataController` | `root:/{action}` | 3 | ✅ | lookup list ทั่วไป |
| `AccountlistController` | `root:/{action}` | 1 | ✅ | รายการบัญชี (lookup) |
| `DocNoController` | `root:/{action}` | 2 | ✅ | running เลขเอกสาร |
| `SystemConfigController` | `root:/{action}` | 8 | ✅ | ตั้งค่าระบบ |
| `TranslationController` | `/api/Translation` | 3 | ⬜ | ข้อความแปลภาษา (i18n) |

---

## 4. ดู Swagger (ราย endpoint)

Guide นี้เป็น index — **รายละเอียด parameter/response ของแต่ละ endpoint ดูที่ Swagger**

- **Swagger UI:** `<base-url>/swagger` (config ที่ `Program.cs:139-140`)
- **Spec JSON:** `<base-url>/swagger/v1/swagger.json`
- Swagger ตั้งค่าให้กด **Authorize** ใส่ JWT ได้ (`SwaggerInstaller.cs`)

> ℹ️ โปรเจกต์ยัง **ไม่ได้เปิด XML doc comments** ใน `.csproj` → คำอธิบายใน Swagger จะยังว่างเป็นส่วนใหญ่
> ถ้าอยากให้ Swagger แสดง description ราย endpoint ให้เพิ่มใน `goalongapi.csproj`:
> ```xml
> <PropertyGroup>
>   <GenerateDocumentationFile>true</GenerateDocumentationFile>
>   <NoWarn>$(NoWarn);1591</NoWarn>
> </PropertyGroup>
> ```
> แล้วชี้ `c.IncludeXmlComments(...)` ใน `SwaggerInstaller.cs` (ปัจจุบัน `NisController` เขียน `<summary>` ไว้ครบแล้ว จะเห็นผลทันที)

---

## 5. หมายเหตุสำหรับผู้ดูแล (Maintainer notes)

1. **ความไม่สม่ำเสมอของ route เป็นเรื่องปกติในโปรเจกต์นี้** — มีทั้ง `api/[controller]`, `[controller]`, literal และ root `/{action}` ปนกัน **อย่าเดา URL** ให้ยืนยันจาก Swagger หรือ attribute ในโค้ดเสมอ
2. **`[action]` = ชื่อ method** และตัวพิมพ์เล็ก/ใหญ่ตามที่ตั้งชื่อ method จริง (เช่น `getProject`, `GetProjectAll`)
3. **Auth ไม่ครบทุก controller** — controller กลุ่ม Service/HR/NIS-adjacent หลายตัวไม่มี `[Authorize]` (ตั้งใจให้ตรงกับ pattern เดิมของ `ServiceTicketsController`) ควรตรวจก่อน expose ออกภายนอก
4. **การตั้งชื่อ controller ใหม่:** แนะนำใช้ `[Route("api/[controller]")]` ให้สม่ำเสมอ + ใส่ `<summary>` doc comment
5. Guide นี้เป็น snapshot — เมื่อเพิ่ม/ลบ controller ให้อัปเดตจำนวนและตารางในหัวข้อ 3

   
   