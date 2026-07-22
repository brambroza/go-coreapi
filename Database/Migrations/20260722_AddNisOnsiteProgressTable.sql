-- ============================================================
-- NIS Onsite — Progress persistence (cross-device draft)
-- เก็บ "ความคืบหน้างาน onsite ที่ค้างอยู่" ต่อ 1 ตั๋ว/1 ช่าง เพื่อเปิดทำต่อข้ามเครื่องได้
-- Contract: go-crm-24v4/docs/nis-onsite-progress-api-contract.md
-- Endpoints: GET/POST/DELETE /api/nis/onsite/:ticketId/progress
-- upsert 1 แถวต่อ (CmpId, TicketId, UserLogin) — last-write-wins ด้วย SavedAt
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisOnsiteProgress', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisOnsiteProgress
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NisOnsiteProgress PRIMARY KEY,
        CmpId nvarchar(100) NOT NULL,
        -- id ตั๋ว onsite ที่ frontend ส่งมา (ServiceTicket.TicketNo หรือ SubTaskId)
        TicketId nvarchar(100) NOT NULL,
        -- ช่างเจ้าของ draft — แยก draft คนละคนบนตั๋วเดียวกัน
        UserLogin nvarchar(200) NOT NULL,
        -- ทั้งก้อน INisOnsiteProgressSnapshot (มี base64 รูป/ลายเซ็นได้ → nvarchar(max))
        SnapshotJson nvarchar(max) NOT NULL,
        -- epoch ms จาก snapshot.savedAt — ใช้เทียบ new/old ตอน reconcile ฝั่ง client
        SavedAt bigint NOT NULL,
        -- server time (audit + ใช้เป็นเกณฑ์ TTL cron ล้าง draft ค้าง)
        UpdatedAt datetime2 NOT NULL
    );
END;

-- unique key ธรรมชาติ = (CmpId, TicketId, UserLogin) → upsert ได้ 1 แถวเสมอ
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteProgress')
      AND name = N'UX_NisOnsiteProgress_CmpId_TicketId_UserLogin'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE UNIQUE INDEX UX_NisOnsiteProgress_CmpId_TicketId_UserLogin
            ON dbo.NisOnsiteProgress (CmpId, TicketId, UserLogin);';
END;

-- index ช่วย cron ล้าง draft ค้าง (WHERE UpdatedAt < cutoff)
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.NisOnsiteProgress')
      AND name = N'IX_NisOnsiteProgress_UpdatedAt'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE INDEX IX_NisOnsiteProgress_UpdatedAt
            ON dbo.NisOnsiteProgress (UpdatedAt);';
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   DROP TABLE IF EXISTS dbo.NisOnsiteProgress;
-- ============================================================
