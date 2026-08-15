---
name: verify
description: ตรวจงานก่อนส่ง/ก่อน merge ใน goalongapi — build, security scan, ตรวจ convention, ผลกระทบต่อ client และ checklist migration ใช้เมื่อทำงานเสร็จก่อนสรุปส่ง หรือก่อน commit
---

# Verify — goalongapi

Definition of Done ของ repo นี้ ต้องผ่านทุกข้อก่อนบอกว่างานเสร็จ

## 1. Build

```bash
dotnet build
```

- ต้องไม่มี error
- ไม่เพิ่ม warning ใหม่โดยไม่จำเป็น (โปรเจคเปิด `Nullable`)
- ไม่มี test project ใน repo — build + ทดสอบมือคือ gate หลัก

## 2. ตรวจ diff ตัวเอง

```bash
git status
git diff
```

ตอบให้ได้: ไฟล์ที่เปลี่ยนทั้งหมดอยู่ในขอบเขต task ใช่ไหม มีไฟล์ที่เผลอแก้ไหม

## 3. Security scan (blocker ทุกข้อ)

```bash
git diff | grep -inE "password|secret|apikey|api_key|connectionstring|bearer |token"
```

- ไม่มี secret / connection string / key หลุดใน diff
- ไม่มี `appsettings*.json`, `token.json`, `client_secret.json`, `config/` ติดมาใน staged files
- SQL ทุกจุดที่รับ input ผู้ใช้เป็น parameterized หรือ stored procedure
- Endpoint ใหม่มี auth ตามที่ควรเป็น และ validate input ครบ
- ไม่ log ข้อมูลอ่อนไหว และไม่ส่ง exception ดิบกลับ client

## 4. Convention

- Route style + response shape ตรงกับ method ข้างเคียงในไฟล์เดียวกัน
- Service ใหม่ชื่อลงท้าย `Service`
- ไม่มี `IHostedService` ถูก register ซ้ำ
- ไม่ได้เขียน `DB.DBConn` static เพิ่มในโค้ดใหม่
- public method มี `/// <summary>` ครบ
- งาน NIS ใช้เวลาไทย (`BangkokNow()`)

## 5. ทดสอบมือ

```bash
dotnet run --launch-profile https   # https://localhost:7046/swagger
```

ยิงผ่าน Swagger ทุกกรณี:
- Happy path
- Input ผิด → คาด 400
- ไม่พบข้อมูล → คาด 404
- สถานะขัดกัน (เช่น approve ซ้ำ) → คาด 409 หรือ error ที่ตั้งใจ
- ถ้ามี realtime/push → ตรวจว่ายิง event จริง
- ถ้าแตะโค้ดที่ endpoint อื่นใช้ร่วม → ยิง endpoint นั้นด้วย (regression)

## 6. Migration

- [ ] มีไฟล์ใน `Database/Migrations/` ตั้งชื่อ `YYYYMMDD_Name.sql`
- [ ] Idempotent + มี header Purpose/Affects/Rollback/Run on
- [ ] Entity / DTO ฝั่ง C# อัพเดตตามแล้ว
- [ ] **แจ้งผู้ใช้ว่ายังไม่ได้รัน** พร้อมลำดับ environment

## 7. ผลกระทบต่อ client

ระบุให้ชัดว่ากระทบตัวไหนและต้องทำอะไร:

| Client | กระทบ | ต้องทำ |
| --- | --- | --- |
| `go-crm-24v4` (Next.js) | ? | ? |
| `NIS-OnsiteService` (React Native) | ? | ? |
| `go-chat-api` (Node.js) | ? | ? |

Breaking change (เปลี่ยนชื่อ route/field, ลบ field, เปลี่ยน auth) ต้องนัดฝั่งนั้นก่อน merge

## 8. สรุปส่งงาน (ภาษาไทย)

1. ทำอะไร — ไฟล์ + บรรทัด
2. Contract ใหม่/ที่เปลี่ยน
3. Migration ที่ต้องรัน
4. ทดสอบอะไรไปบ้าง ผลเป็นอย่างไร
5. อะไรยังไม่ได้ทำ / สมมติฐาน / ความเสี่ยงที่เหลือ

ถ้าข้อไหนไม่ผ่าน ให้บอกตรง ๆ พร้อม output จริง อย่ารายงานว่าเสร็จทั้งที่ยังไม่ผ่าน
