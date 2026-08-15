---
name: db-migration
description: เขียน migration SQL สำหรับ goalongapi ให้ idempotent ปลอดภัย และ backward compatible พร้อมอัพเดต entity/DTO ฝั่ง C# ใช้เมื่อต้องเพิ่ม/แก้ตาราง คอลัมน์ index หรือ stored procedure
---

# DB Migration — goalongapi

## กฎเหล็ก

- Migration ของ repo นี้คือ **ไฟล์ SQL เขียนมือ** ใน `Database/Migrations/` ไม่มี EF migration snapshot
- **Claude ห้ามรัน SQL บน database ใด ๆ** — เขียนสคริปต์ให้ผู้ใช้รันเอง แล้วบอกลำดับ
- ห้ามรันบน production โดยไม่มี approval

## ขั้นตอน

### 1. สำรวจก่อน

```bash
ls Database/Migrations/                                  # ดูรูปแบบและอันล่าสุด
grep -rn "ชื่อตาราง" Controllers/ Models/ Data/ Datatools/  # หาว่าใครใช้ตารางนี้บ้าง
```

ตอบให้ได้ก่อนเขียน: ตารางนี้ถูกอ่าน/เขียนที่ไหนบ้าง มี SP ไหนอ้างถึง จะพังตรงไหนถ้าเปลี่ยน

### 2. เลือกวิธีที่ปลอดภัยที่สุด

| ต้องการ | ทำแบบ |
| --- | --- |
| เพิ่มข้อมูลใหม่ | `ADD COLUMN ... NULL` (มี default ถ้าจำเป็น) — ปลอดภัยสุด |
| เปลี่ยนชนิดคอลัมน์ | เพิ่มคอลัมน์ใหม่ → backfill → ให้โค้ดใช้ตัวใหม่ → ค่อยลบตัวเก่าใน sprint ถัดไป |
| เปลี่ยนชื่อคอลัมน์ | เท่ากับ breaking change — ต้องแจ้งและแก้ทุกจุดที่อ้างถึงพร้อมกัน |
| ลบคอลัมน์/ตาราง | ต้องได้ approval ชัดเจน และตรวจว่าไม่มีที่ไหนอ้างแล้วจริง |

หลักการ: **เพิ่มได้ ลบยาก** ทำให้ deploy โค้ดใหม่กับ schema ใหม่ไม่ต้องพร้อมกันเป๊ะ

### 3. เขียนไฟล์

ชื่อไฟล์: `Database/Migrations/YYYYMMDD_ShortPascalCaseName.sql`

```sql
-- 20260814_AddTicketRescheduleReason.sql
-- Purpose : เก็บเหตุผลการเลื่อนวันปฏิบัติงานของตั๋ว NIS
-- Affects : dbo.ServiceTicket
-- Rollback: ALTER TABLE dbo.ServiceTicket DROP COLUMN RescheduleReason;
-- Run on  : dev -> staging -> production (แจ้งทีมก่อนรัน production)

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ServiceTicket') AND name = 'RescheduleReason'
)
BEGIN
    ALTER TABLE dbo.ServiceTicket ADD RescheduleReason NVARCHAR(500) NULL;
END
GO
```

บังคับ: header comment ครบ 5 บรรทัด (Purpose / Affects / Rollback / Run on) + idempotent ทุก statement

### 4. ตามด้วยฝั่ง C#

- อัพเดต entity/model ใน `Models/` หรือ `Entities/`
- อัพเดต DTO ที่เกี่ยวใน `Dtos/`
- ถ้ามี SP ที่ต้องแก้ ให้เขียนเป็น `CREATE OR ALTER PROCEDURE` ในไฟล์ migration เดียวกัน
- `dotnet build` ต้องผ่าน

### 5. รายงาน (ภาษาไทย, บังคับ)

```
Migration: Database/Migrations/20260814_AddTicketRescheduleReason.sql
สถานะ:     ⚠️ ยังไม่ได้รันบน database ใด ๆ
ลำดับรัน:  dev → staging → production
Rollback:  ระบุไว้หัวไฟล์
โค้ดที่แก้ตาม: Models/Nis/ServiceTicket.cs:42, Dtos/nis/NisDtos.cs:88
ผลกระทบ:   go-crm-24v4 จะเห็น field ใหม่ใน response (เพิ่มอย่างเดียว ไม่ breaking)
```

## Index / performance

เพิ่ม index ต้องบอก: query ไหนที่ช้า, index ที่เสนอครอบ column อะไรตามลำดับใด, ผลกระทบต่อ write และขนาด storage
