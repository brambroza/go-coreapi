---
name: api-endpoint
description: เพิ่มหรือแก้ REST endpoint ใน goalongapi ให้ตรง convention ของ controller เดิม พร้อม DTO, validation และเอกสาร contract ให้ client ใช้ต่อ ใช้เมื่องานคือการเพิ่ม/แก้ API
---

# API Endpoint — goalongapi

## 1. ก่อนเขียน

1. อ่าน controller เป้าหมายทั้งไฟล์ (หรืออย่างน้อย 200 บรรทัดรอบจุดที่จะแก้)
2. จด pattern ที่ใช้จริงในไฟล์นั้น:
   - Route: `[Route("[controller]")]` / `[Route("api/[controller]")]` / explicit `[Route("api/ชื่อ")]`
   - Auth: มี `[Authorize]` ไหม (`NisController`, `ServiceTicketsController` ปัจจุบันไม่มี — เป็นของเดิม)
   - Data access: EF Core (`DatabaseContext` / `HrDbContext`) หรือ `DbConnectionFactory` หรือ `DBConn` เก่า
   - Response shape: `MsgReturn` / raw JSON string / typed DTO / anonymous object
3. ตรวจว่ามี endpoint ที่ทำเรื่องเดียวกันอยู่แล้วไหม (`grep -rn "api/คำที่เกี่ยว" Controllers/`)

**ทำตาม pattern ของไฟล์นั้น ไม่ใช่ pattern ที่คิดว่าดีที่สุด** — ความสม่ำเสมอในไฟล์สำคัญกว่า

## 2. โครงที่ควรได้

```csharp
/// <summary>
/// อธิบายว่า endpoint นี้ทำอะไร ใครเรียก และ side effect คืออะไร
/// </summary>
/// <param name="dto">ข้อมูลที่รับเข้ามา</param>
/// <returns>ผลลัพธ์ ...</returns>
[HttpPost("reschedule")]
public async Task<IActionResult> RescheduleTicket([FromBody] RescheduleTicketDto dto)
{
    if (dto is null || dto.TicketId <= 0)
        return BadRequest(new { message = "ticketId ไม่ถูกต้อง" });

    // ... logic
}
```

- DTO อยู่ใน `Dtos/<module>/` ไม่รับ entity ตรง ๆ
- Validate ทุก field: null, ช่วงค่า, ความยาว, สิทธิ์ผู้เรียก
- Error ตอบด้วย status code ที่ถูกต้อง (400 input ผิด, 404 ไม่พบ, 409 ขัดสถานะ, 500 เฉพาะ error จริง)
- ห้าม return exception message ดิบให้ client — log ไว้ฝั่ง server แทน

## 3. Data access

- โค้ดใหม่: EF Core หรือ `DbConnectionFactory` เท่านั้น
- Input ผู้ใช้เข้า SQL ต้องผ่าน `SqlParameter` หรือ stored procedure เสมอ
- งานที่แตะหลายตารางต้องอยู่ใน transaction เดียว

## 4. ห้ามทำโดยไม่ขออนุญาต

- เปลี่ยนชื่อ route หรือ action ของ endpoint เดิม
- ลบ / เปลี่ยนชนิด field ใน response ที่มีอยู่ (เพิ่ม field ใหม่ทำได้ ถือว่า backward compatible)
- เปลี่ยน auth ของ endpoint เดิม
- ทั้งสามข้อกระทบ `go-crm-24v4`, `NIS-OnsiteService`, `go-chat-api`

## 5. ส่งมอบ contract

ทุกครั้งที่เพิ่ม/แก้ endpoint ต้องแนบให้ฝั่ง client เอาไปใช้ได้ทันที:

```
POST /api/nis/tickets/reschedule
Auth: none (ตาม pattern เดิมของ controller)

Request
{ "ticketId": 123, "newDate": "2026-08-20", "reason": "ลูกค้าขอเลื่อน" }

Response 200
{ "success": true, "ticketId": 123, "onsiteDate": "2026-08-20T00:00:00" }

Response 400 { "message": "ticketId ไม่ถูกต้อง" }
Response 404 { "message": "ไม่พบตั๋ว" }

Side effect: re-sync Google Calendar เฉพาะตั๋วที่ assign แล้ว, ส่ง push หาผู้รับผิดชอบ
```

## 6. ทดสอบ

`dotnet build` แล้วยิงผ่าน Swagger: happy path, validation ผิด, ไม่พบข้อมูล, สิทธิ์ไม่พอ
