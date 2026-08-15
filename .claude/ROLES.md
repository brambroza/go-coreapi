# ROLES — goalongapi (GoAlong Core API)

ตารางบทบาทสำหรับสั่งงานผ่าน Claude Code ใน repo นี้
ใช้คู่กับ [../CLAUDE.md](../CLAUDE.md) และ agent ใน [agents/](agents/)

---

## 1. Role matrix

| Role | Agent | ขอบเขต | ห้ามทำ |
| --- | --- | --- | --- |
| **Backend Developer** | `coreapi-backend` | Controller, Service, DTO, business logic, integration (Google / LINE / Expo) | แก้ schema, deploy, เปลี่ยน route เดิม |
| **Database Engineer** | `coreapi-dba` | Schema design, migration SQL, stored procedure, query tuning, index | รัน SQL บน DB จริง, ลบ/แก้ข้อมูล |
| **Realtime & Infra** | `coreapi-realtime` | SignalR hub, RabbitMQ, hosted service, push notification, Docker, CI | เปลี่ยน production config, deploy |
| **Code Reviewer** | `coreapi-reviewer` | Review diff / PR, security, performance, convention | แก้โค้ดเอง (รายงานอย่างเดียว) |
| **Explorer** | `coreapi-explorer` | ค้นหาโค้ด, map flow, ตอบ "ของนี้อยู่ไหน" | แก้ไฟล์ใด ๆ |
| **Project Manager** | `project-manager` (global) | Sprint plan, breakdown, estimate, timeline | ตัดสินใจ technical design |
| **Solution Architect** | `solution-architect` (global) | Architecture decision, tech trade-off | ลงมือ implement เอง |

---

## 2. เลือก role อย่างไร

```
คำสั่งเข้ามา
├── "อยู่ไหน / ใครเรียก / flow เป็นยังไง"      → coreapi-explorer
├── "วางแผน sprint / ประเมินเวลา"              → /sprint-plan + project-manager
├── "ทำ feature ตาม spec"                      → /task-feature + coreapi-backend
├── "แก้บั๊ก / ระบบพัง"                        → /fixbug + coreapi-backend
├── "เพิ่ม endpoint"                           → /api-endpoint + coreapi-backend
├── "เพิ่ม column / ตาราง / query ช้า"          → /db-migration + coreapi-dba
├── "notification ไม่เข้า / hub / queue / deploy" → coreapi-realtime
├── "review ให้หน่อย / ก่อน merge"              → coreapi-reviewer
└── "ควรออกแบบยังไงดี"                          → solution-architect
```

---

## 3. Escalation

| สถานการณ์ | ต้องแจ้ง / ขอ approve |
| --- | --- |
| ต้องเปลี่ยน schema | ผู้ใช้ + ทีม DBA ก่อนเขียน migration |
| ต้องเปลี่ยน route หรือ response shape ของ endpoint เดิม | ผู้ใช้ + เจ้าของ client (`go-crm-24v4`, `NIS-OnsiteService`, `go-chat-api`) |
| งานเกิน scope ที่สั่ง | หยุดแล้วเสนอเป็น Change Request ระบุ scope + เวลา + ราคาที่เพิ่ม |
| เจอช่องโหว่ security | รายงานทันที ก่อนทำงานอื่นต่อ |
| ต้องรัน migration / แตะ DB จริง | ห้ามรันเอง แจ้งผู้ใช้พร้อมสคริปต์และลำดับการรัน |
| ต้อง deploy / push | ขออนุญาตทุกครั้ง |
| Blocker กระทบ timeline | แจ้งภายใน 4 ชั่วโมงพร้อมทางเลือก |

---

## 4. Definition of Done (ทุก role)

1. `dotnet build` ผ่าน ไม่มี error ใหม่ และไม่เพิ่ม warning โดยไม่จำเป็น
2. โค้ดใหม่มี XML doc comment และ input validation ครบ
3. Query ที่รับ input ผู้ใช้ทั้งหมดเป็น parameterized หรือ stored procedure
4. ไม่มี secret หลุดใน diff
5. ถ้ามี migration — เขียนไฟล์ใน `Database/Migrations/` แบบ idempotent และ **แจ้งว่ายังไม่ได้รัน**
6. ถ้ากระทบ client — ระบุชัดว่ากระทบตัวไหน ต้องแก้อะไรฝั่งนั้น
7. สรุปงานเป็นภาษาไทย: แก้อะไร ที่ไฟล์ไหน ทดสอบยังไง อะไรยังไม่ได้ทำ
