---
name: fixbug
description: แก้บั๊กใน goalongapi ตั้งแต่ triage, reproduce, หา root cause, fix แบบแคบที่สุด จนถึงตรวจ regression ใช้เมื่อได้รับ bug report จากลูกค้า ทีม หรือ error จาก production
---

# Fix Bug — goalongapi

## Phase 1 — Triage

เก็บให้ครบก่อนแก้ ถ้าขาดให้ถาม:
- อาการที่เห็น กับ ที่คาดว่าควรเป็น
- Client ตัวไหน (`go-crm-24v4` / `NIS-OnsiteService` / `go-chat-api` / ยิง API ตรง)
- Endpoint + payload + response ที่ได้ (รวม HTTP status)
- Environment (local / staging / production) และเวลาที่เกิด
- เกิดทุกครั้งหรือบางครั้ง / เพิ่งเกิดหลัง deploy ไหน
- Error message และ stack trace เต็ม

จัดระดับตาม SLA ของ GoAlong:

| Priority | นิยาม | Response / Resolve |
| --- | --- | --- |
| P1 | ระบบใช้ไม่ได้ / ข้อมูลเสียหาย / security | 1 ชม. / 24 ชม. |
| P2 | feature หลักพัง แต่มีทางเลี่ยง | 4 ชม. / 72 ชม. |
| P3 | ปัญหาย่อย, cosmetic | 24 ชม. / sprint ถัดไป |

P1 ต้องแจ้ง Technical Lead และ Management ทันที

## Phase 2 — Reproduce

- หา endpoint ที่เกี่ยวด้วย Grep จาก URL ใน bug report
- ยิงซ้ำผ่าน Swagger ด้วย payload เดียวกัน
- ถ้าซ้ำไม่ได้ ให้ระบุว่าต่างกันตรงไหน (ข้อมูล, สิทธิ์, สถานะ, timezone) แล้วขอข้อมูลเพิ่ม
- **ห้ามแก้โค้ดก่อนอธิบายสาเหตุได้** — ถ้ายังไม่รู้สาเหตุ ให้บอกตรง ๆ ว่ายังไม่รู้ อย่าเดาแล้วแก้มั่ว

## Phase 3 — หา root cause

จุดที่พังบ่อยใน repo นี้:

| อาการ | ที่มักเป็นสาเหตุ |
| --- | --- |
| ค่าที่บันทึกผิดเวลา / คลาดไป 7 ชม. | ใช้ `DateTime.Now` แทน `BangkokNow()` ในงาน NIS |
| ข้อมูลค้างครึ่ง ๆ | transaction ไม่ครอบครบ หรือ rollback ไม่ทำงาน |
| ผลลัพธ์สลับกันข้าม request | `DB/DBConn.cs` ใช้ static connection/transaction ร่วมกัน — ไม่ thread-safe |
| งาน background ทำซ้ำ 2 รอบ | `IHostedService` ถูก register ซ้ำ |
| แจ้งเตือนไม่เข้า | แยก path foreground (`NisRealtimeNotifyService` → go-chat-api) กับ background (`ExpoPushService`) |
| Client เรียกไม่ได้ / CORS | origin ไม่อยู่ใน whitelist ที่ `Program.cs` |
| 500 ตอนอัปโหลดไฟล์ใหญ่ | เกิน `NisOnsite:MaxRequestBodyBytes` หรือ `FileSizeLimit` |
| Column not found ตอนรัน | migration ใน `Database/Migrations/` ยังไม่ถูกรันบน environment นั้น |
| ข้อมูลถูกแต่ client เห็นผิด | response shape ต่างจากที่ client คาด (มีหลาย shape ในระบบ) |

ตรวจด้วยว่า bug เดียวกันมีอยู่ที่อื่นอีกไหม — โค้ดในระบบนี้ซ้ำกันเยอะ ให้ Grep pattern เดียวกันทั้ง repo แล้วรายงาน

## Phase 4 — Fix

- แก้ที่ **root cause** ไม่ใช่กลบอาการ ถ้าจำเป็นต้อง workaround ให้บอกชัดว่าเป็น workaround และหนี้ที่เหลือคืออะไร
- แก้ให้แคบที่สุด — ห้าม refactor พ่วง
- ถ้าเจอบั๊กเดียวกันหลายจุด: แก้จุดที่รายงานก่อน แล้วรายงานจุดอื่นให้ผู้ใช้ตัดสินใจ ว่าจะรวมใน task นี้หรือแยก
- ถ้าต้องแก้ schema → ไปที่ `/db-migration`

## Phase 5 — Verify + regression

```bash
dotnet build
```

- ยิง case ที่เคยพัง → ต้องหาย
- ยิง happy path ของ endpoint เดียวกัน → ต้องยังทำงาน
- ยิง endpoint อื่นที่ใช้โค้ด/ตารางเดียวกัน → ตรวจ regression
- ถ้าแก้ที่ shared helper ให้ list ผู้ใช้ helper นั้นทั้งหมด (`grep -rn`) แล้วประเมินผลกระทบ

## Phase 6 — รายงาน (ภาษาไทย)

```
อาการ:      ...
สาเหตุ:     path/file.cs:123 — ...
แก้:        path/file.cs:123 — ...
ทดสอบ:      ... ผล ...
Regression: ตรวจ ... แล้ว ไม่พบปัญหา
เหลือ:      จุดเดียวกันยังมีที่ path/other.cs:88 — ยังไม่แก้ รอตัดสินใจ
```

commit type ใช้ `fix(<scope>): ...` — แต่ห้าม commit จนกว่าผู้ใช้จะสั่ง
