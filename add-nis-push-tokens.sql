-- ════════════════════════════════════════════════════════════════════
-- NIS Onsite push notification (Track B MVP) — Expo Push token registry + send log
--   NisPushToken : token ต่อ (CmpId, StaffName, DeviceId) — upsert ตอน app start/login
--   NisPushLog   : กันยิงซ้ำ (dedupe) ด้วย EventKey unique + เก็บประวัติที่ส่งแล้ว
-- ⚠️ รันด้วยมือหลังแจ้งทีม (ตามกฎ database migration) — idempotent รันซ้ำได้
-- ════════════════════════════════════════════════════════════════════

IF OBJECT_ID('dbo.NisPushToken', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisPushToken
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CmpId NVARCHAR(100) NOT NULL,
        -- Account.FullName — ตรงกับ NisTicket.Assignee (identity เดียวกับที่ใช้ assign งาน)
        StaffName NVARCHAR(200) NOT NULL,
        UserId NVARCHAR(100) NULL,
        ExpoPushToken NVARCHAR(255) NOT NULL,
        -- UUID ที่ app สร้างครั้งแรกแล้วเก็บใน AsyncStorage — ช่างหลายเครื่อง = หลายแถว
        DeviceId NVARCHAR(255) NOT NULL,
        Platform NVARCHAR(20) NULL,      -- ios | android
        AppVersion NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_NisPushToken_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_NisPushToken_UpdatedAt DEFAULT SYSUTCDATETIME()
    );

    -- upsert key: เครื่องเดิม + คนเดิม = แถวเดิม (token เปลี่ยนได้หลัง reinstall/OS update)
    CREATE UNIQUE INDEX UX_NisPushToken_StaffDevice
        ON dbo.NisPushToken (CmpId, StaffName, DeviceId);
END

IF OBJECT_ID('dbo.NisPushLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisPushLog
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        -- dedupe key เช่น 'assign:TICKET_ID:ชื่อช่าง:202607161530' / 'overdue:TICKET_ID:20260716'
        EventKey NVARCHAR(255) NOT NULL,
        CmpId NVARCHAR(100) NULL,
        TicketId NVARCHAR(100) NULL,
        StaffName NVARCHAR(200) NULL,
        Title NVARCHAR(255) NULL,
        Body NVARCHAR(500) NULL,
        SentAt DATETIME2 NOT NULL CONSTRAINT DF_NisPushLog_SentAt DEFAULT SYSUTCDATETIME()
    );

    -- unique = insert ซ้ำล้ม → service ข้ามการส่ง (first-writer-wins)
    CREATE UNIQUE INDEX UX_NisPushLog_EventKey
        ON dbo.NisPushLog (EventKey);
END
