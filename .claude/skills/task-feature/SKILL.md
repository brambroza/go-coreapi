---
name: task-feature
description: ทำ task หรือ feature ใน goalongapi ตั้งแต่สำรวจ วางแผน implement จนถึง build และสรุปส่งงาน ใช้เมื่อได้รับ task จาก sprint หรือสั่งให้ทำ feature ใหม่
---

# Task / Feature — goalongapi

ลำดับงานมาตรฐานสำหรับ implement 1 task ใน `goalongapi`

## Phase 1 — สำรวจ (ห้ามข้าม)

1. อ่าน `CLAUDE.md` หัวข้อ Module map หา domain ที่เกี่ยว
2. หาไฟล์เป้าหมายด้วย agent `coreapi-explorer` หรือ Grep เอง — ต้องได้ `path:line` ชัด
3. อ่าน method ข้างเคียงในไฟล์เดียวกัน เพื่อจับ pattern: route style, response shape, auth, EF Core หรือ `DBConn`
4. เช็กว่ามี endpoint ที่ทำเรื่องเดียวกันอยู่แล้วหรือยัง — repo นี้มีโค้ดซ้ำเยอะ

## Phase 2 — วางแผนและยืนยัน

บอกผู้ใช้ก่อนลงมือ (สั้น ๆ):
- ไฟล์ที่จะแก้ / สร้าง
- ต้องแก้ schema ไหม (ถ้าต้อง → หยุด ไปใช้ `/db-migration` ก่อน แล้วค่อยกลับมา)
- Route + request/response contract ที่จะได้
- กระทบ client ตัวไหน (`go-crm-24v4`, `NIS-OnsiteService`, `go-chat-api`)

ถ้า requirement ไม่ชัดในจุดที่ตัดสินใจต่างกันแล้วงานออกมาต่างกัน — **ถามก่อน**

## Phase 3 — Implement

ลำดับ: Migration → Entity/Model → DTO → Service → Controller → Realtime/Notify

กฎ:
- Minimal diff — แก้เฉพาะที่จำเป็น ห้าม reformat หรือ refactor รอบข้าง
- Parameterized query หรือ SP เท่านั้นสำหรับ input ผู้ใช้
- โค้ดใหม่ใช้ EF Core หรือ `DbConnectionFactory` ห้ามเขียน `DB.DBConn` static เพิ่ม
- Service ใหม่ชื่อลงท้าย `Service` · ห้าม register `IHostedService` ซ้ำ
- Validate input ครบทุก field · ใส่ `/// <summary>` ทุก public method
- งาน NIS ใช้เวลาไทย — `BangkokNow()` ไม่ใช่ `DateTime.Now`
- Notify / push เป็น best-effort — ครอบ try/catch ไม่ให้ล้มกระทบงานหลัก

## Phase 4 — Verify

```bash
dotnet build
```

ต้องผ่าน ไม่มี error ใหม่ แล้วทดสอบด้วยมือ:
- ยิง endpoint ผ่าน Swagger (`https://localhost:7046/swagger`) — happy path + error case
- ถ้ามี migration: บอกว่ายังไม่รัน และลำดับการรันคืออะไร
- ถ้ามี realtime: ตรวจว่า event ยิงออกจริง

รายละเอียดเพิ่มเติมดู `/verify`

## Phase 5 — สรุปส่งงาน (ภาษาไทย)

1. ทำอะไรไปบ้าง — แต่ละไฟล์ `path:line`
2. Contract ใหม่ — method + route + request/response ตัวอย่าง (ให้ฝั่ง client เอาไปใช้ได้เลย)
3. ต้องรัน migration อะไร ที่ไหน
4. ทดสอบยังไง ผลเป็นยังไง
5. อะไรยังไม่ได้ทำ / ข้อจำกัด / สมมติฐานที่ตั้งไว้

**ห้าม commit หรือ push เอง** — รอผู้ใช้สั่ง ถ้าผู้ใช้สั่ง ให้ใช้ Conventional Commits
เช่น `feat(nis): add ticket reschedule endpoint`
