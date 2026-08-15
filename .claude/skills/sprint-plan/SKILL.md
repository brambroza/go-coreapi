---
name: sprint-plan
description: วางแผน sprint ของ goalongapi — แตก requirement เป็น task ระดับไฟล์ ประเมินชั่วโมง จัดลำดับ และหา dependency ใช้เมื่อเริ่ม sprint ใหม่หรือรับ requirement ก้อนใหญ่
---

# Sprint Plan — goalongapi

แปลง requirement เป็นแผนงานที่สั่ง Claude Code ทำต่อทีละ task ได้

## ขั้นตอน

### 1. เก็บ requirement ให้ครบก่อน

ถามให้ครบก่อนวางแผน อย่าเดา:
- ผู้ใช้ปลายทางคือใคร (CRM web / NIS mobile / ทั้งคู่)
- Business rule ที่สำคัญ (ใครอนุมัติได้, สถานะไหนแก้ได้, ตัดสต็อกตอนไหน)
- Deadline และ demo date
- มี design/mockup ฝั่ง frontend แล้วหรือยัง

### 2. สำรวจของเดิม (บังคับ)

ก่อนประเมิน ต้องรู้ว่ามีของเดิมให้ใช้ซ้ำหรือไม่ ใช้ agent `coreapi-explorer` หา:
- Controller / endpoint ที่ทำงานใกล้เคียงอยู่แล้ว
- ตารางและคอลัมน์ที่มีอยู่ รองรับ requirement ได้ไหม
- Client ตัวไหนเรียก endpoint ที่จะแตะบ้าง

### 3. แตก task

แตกให้แต่ละ task **ทำเสร็จได้ใน 1 วันทำงาน** และระบุไฟล์เป้าหมายชัด

| ฟิลด์ | ตัวอย่าง |
| --- | --- |
| ID | `NIS-12` |
| หัวข้อ | เพิ่ม endpoint เลื่อนวันปฏิบัติงานของตั๋ว |
| ไฟล์ที่แตะ | `Controllers/NisController.cs`, `Dtos/nis/NisDtos.cs` |
| Layer | DB / API / Integration / Realtime |
| ประเมิน (ชม.) | 6 |
| Depends on | `NIS-11` (migration คอลัมน์ใหม่) |
| Acceptance | ยิง POST แล้ววันเปลี่ยน + calendar sync เฉพาะตั๋วที่ assign แล้ว |

จัดลำดับตาม layer เสมอ: **Migration → Entity/DTO → Service/Logic → Controller → Realtime/Notify → เอกสาร**

### 4. ประเมินเวลา

ฐาน (ชั่วโมง) สำหรับ repo นี้:

| งาน | ประเมิน |
| --- | --- |
| Endpoint CRUD ธรรมดา + DTO | 3–5 |
| Endpoint ที่มี business rule / transaction หลายตาราง | 6–12 |
| Migration + แก้ entity/DTO ตาม | 2–4 |
| แก้ controller ใหญ่ (>2,000 บรรทัด) | +50% จากงานปกติ เพราะเสี่ยง regression |
| Integration ใหม่ (Google / LINE / Expo) | 8–16 |
| Realtime event ใหม่ (hub + client contract) | 6–10 |
| Debug บั๊กที่ยังไม่รู้สาเหตุ | ตั้ง timebox 4 ชม. แล้วรายงาน |

บวก buffer ตาม CLAUDE.md ระดับ org: SME client +20%, requirement ไม่ชัด +15%, เทคโนโลยีใหม่ +25%

### 5. ระบุความเสี่ยง

อย่างน้อยต้องตอบ:
- Task ไหนต้องแก้ schema (ต้องหา window รัน migration)
- Task ไหนเปลี่ยน contract ที่กระทบ client — ต้องนัดฝั่ง frontend/mobile
- Task ไหนแตะ controller ยักษ์ (`ProjectController`, `DocNoController`, `NisController`, `CrmController`, `ServiceTicketsController`)
- ไม่มี automated test ในโปรเจค — ทุก task ต้องมีขั้นตอนทดสอบมือระบุไว้

### 6. Output

เขียนเป็นตารางภาษาไทย + สรุปท้าย:
- รวมชั่วโมง / รวมวันทำงาน
- ลำดับที่แนะนำให้ทำ
- สิ่งที่ยังต้องการคำตอบจากลูกค้าก่อนเริ่ม
- คำสั่งที่ใช้ทำ task แต่ละตัวต่อ (เช่น `/task-feature NIS-12`)

ถ้าผู้ใช้ต้องการ ให้เขียนแผนลงไฟล์ `docs/sprint/<sprint-name>.md` (ถามก่อนสร้างโฟลเดอร์ใหม่)
