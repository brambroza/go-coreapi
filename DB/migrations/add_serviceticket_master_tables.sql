-- ============================================================
-- Migration: Service Ticket Master Tables
-- Date: 2026-06-24
-- Description: สร้างตาราง master data สำหรับ ServiceTicket
--              (Category, Tag, Checklist) พร้อม seed ข้อมูลเริ่มต้น
-- ============================================================

-- ─── ServiceTicketMasterCategory ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceTicketMasterCategory')
BEGIN
    CREATE TABLE [dbo].[ServiceTicketMasterCategory] (
        [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name]      NVARCHAR(100)     NOT NULL,
        [Seq]       INT               NOT NULL DEFAULT 0,
        [IsActive]  BIT               NOT NULL DEFAULT 1,
        [CmpId]     NVARCHAR(50)      NULL,
        [UpdUser]   NVARCHAR(100)     NULL,
        [CreatedAt] DATETIME          NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME          NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table ServiceTicketMasterCategory';
END
ELSE
    PRINT 'Table ServiceTicketMasterCategory already exists';

-- ─── ServiceTicketMasterTag ──────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceTicketMasterTag')
BEGIN
    CREATE TABLE [dbo].[ServiceTicketMasterTag] (
        [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name]      NVARCHAR(100)     NOT NULL,
        [Seq]       INT               NOT NULL DEFAULT 0,
        [IsActive]  BIT               NOT NULL DEFAULT 1,
        [CmpId]     NVARCHAR(50)      NULL,
        [UpdUser]   NVARCHAR(100)     NULL,
        [CreatedAt] DATETIME          NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME          NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table ServiceTicketMasterTag';
END
ELSE
    PRINT 'Table ServiceTicketMasterTag already exists';

-- ─── ServiceTicketMasterChecklist ────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceTicketMasterChecklist')
BEGIN
    CREATE TABLE [dbo].[ServiceTicketMasterChecklist] (
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ChecklistType] NVARCHAR(50)      NOT NULL,  -- implement | ma
        [Name]          NVARCHAR(500)     NOT NULL,
        [Seq]           INT               NOT NULL DEFAULT 0,
        [IsActive]      BIT               NOT NULL DEFAULT 1,
        [CmpId]         NVARCHAR(50)      NULL,
        [UpdUser]       NVARCHAR(100)     NULL,
        [CreatedAt]     DATETIME          NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]     DATETIME          NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Created table ServiceTicketMasterChecklist';
END
ELSE
    PRINT 'Table ServiceTicketMasterChecklist already exists';

-- ─── Seed: Categories ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ServiceTicketMasterCategory])
BEGIN
    INSERT INTO [dbo].[ServiceTicketMasterCategory] ([Name], [Seq], [IsActive]) VALUES
    (N'Runrate',     1, 1),
    (N'Implement',   2, 1),
    (N'MA-Device',   3, 1),
    (N'MA-Fortigate',4, 1),
    (N'MA-Software', 5, 1),
    (N'MA-Network',  6, 1);
    PRINT 'Seeded ServiceTicketMasterCategory';
END

-- ─── Seed: Tags ──────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ServiceTicketMasterTag])
BEGIN
    INSERT INTO [dbo].[ServiceTicketMasterTag] ([Name], [Seq], [IsActive]) VALUES
    (N'Firewall',       1,  1),
    (N'Network',        2,  1),
    (N'WiFi',           3,  1),
    (N'Server',         4,  1),
    (N'CCTV',           5,  1),
    (N'Access Control', 6,  1),
    (N'PC&Notebook',    7,  1),
    (N'Software',       8,  1),
    (N'Cable',          9,  1),
    (N'Windows Server', 10, 1),
    (N'VMware',         11, 1);
    PRINT 'Seeded ServiceTicketMasterTag';
END

-- ─── Seed: Checklists ────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[ServiceTicketMasterChecklist])
BEGIN
    -- Implement checklists
    INSERT INTO [dbo].[ServiceTicketMasterChecklist] ([ChecklistType], [Name], [Seq], [IsActive]) VALUES
    (N'implement', N'ตรวจสอบรายการสินค้า / อุปกรณ์ครบถ้วน',       1,  1),
    (N'implement', N'ดำเนินการ PreConfig อุปกรณ์ก่อนออกงาน',       2,  1),
    (N'implement', N'ติดตั้ง Rack / ขึ้นแร็ค',                      3,  1),
    (N'implement', N'เดินสาย Fiber / UTP',                           4,  1),
    (N'implement', N'Config Network Address / VLAN',                  5,  1),
    (N'implement', N'Config ระบบ Firewall Policy',                   6,  1),
    (N'implement', N'ทดสอบการเชื่อมต่อ Internet / WAN',              7,  1),
    (N'implement', N'ทดสอบ Internal Network',                         8,  1),
    (N'implement', N'จัดทำ Network Diagram ตาม AS-BUILT',            9,  1),
    (N'implement', N'บันทึก IP / User / Password เข้าระบบ',         10, 1),
    (N'implement', N'ส่งมอบงานและให้ลูกค้าเซ็นรับ',                11, 1);

    -- MA checklists
    INSERT INTO [dbo].[ServiceTicketMasterChecklist] ([ChecklistType], [Name], [Seq], [IsActive]) VALUES
    (N'ma', N'ตรวจสอบ Log / Event ย้อนหลัง',               1, 1),
    (N'ma', N'ตรวจสอบ CPU / Memory / Disk Usage',           2, 1),
    (N'ma', N'Update Firmware / Signature ล่าสุด',          3, 1),
    (N'ma', N'ตรวจสอบ HA Cluster / Failover',               4, 1),
    (N'ma', N'Remote Backup Config',                         5, 1),
    (N'ma', N'ทดสอบ Failover System',                        6, 1),
    (N'ma', N'บันทึกผลการตรวจสอบลง Monthly Report',         7, 1);

    PRINT 'Seeded ServiceTicketMasterChecklist';
END
