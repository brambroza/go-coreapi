-- ============================================================
-- Migration: Warranty Claims & Device Tables
-- Date: 2026-06-25
-- Description: สร้างตาราง WarrantyDevice (ข้อมูลรับประกันอุปกรณ์)
--              และ WarrantyClaim (ใบแจ้งเคลม) พร้อม seed ข้อมูลเริ่มต้น
-- ============================================================

-- ─── WarrantyDevice ──────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WarrantyDevice')
BEGIN
    CREATE TABLE [dbo].[WarrantyDevice] (
        [Id]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SerialNo]       NVARCHAR(100)     NOT NULL,
        [ProductName]    NVARCHAR(200)     NOT NULL,
        [Brand]          NVARCHAR(100)     NOT NULL,
        [Model]          NVARCHAR(100)     NOT NULL,
        [Customer]       NVARCHAR(200)     NULL,
        [ProjectNo]      NVARCHAR(50)      NULL,
        [WarrantyStatus] NVARCHAR(10)      NOT NULL DEFAULT 'on',   -- on | off
        [WarrantyExpiry] DATE              NULL,
        [CmpId]          NVARCHAR(50)      NULL,
        [UpdUser]        NVARCHAR(100)     NULL,
        [CreatedAt]      DATETIME          NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]      DATETIME          NOT NULL DEFAULT GETDATE()
    );

    CREATE UNIQUE INDEX UX_WarrantyDevice_SerialNo
        ON [dbo].[WarrantyDevice]([SerialNo]);

    PRINT 'Created table WarrantyDevice';
END
ELSE
    PRINT 'Table WarrantyDevice already exists';

-- ─── WarrantyClaim ───────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WarrantyClaim')
BEGIN
    CREATE TABLE [dbo].[WarrantyClaim] (
        [Id]             NVARCHAR(30)      NOT NULL PRIMARY KEY,  -- CLM-YYYY-NNNN
        [TicketId]       NVARCHAR(100)     NULL,
        [Customer]       NVARCHAR(200)     NOT NULL,
        [SalesName]      NVARCHAR(100)     NULL,
        [ReporterStaff]  NVARCHAR(100)     NOT NULL,
        [Brand]          NVARCHAR(100)     NOT NULL,
        [Model]          NVARCHAR(100)     NOT NULL,
        [SerialNo]       NVARCHAR(100)     NOT NULL,
        [WarrantyStatus] NVARCHAR(10)      NOT NULL DEFAULT 'on',
        [Status]         NVARCHAR(50)      NOT NULL DEFAULT 'Claim Received',
        [Detail]         NVARCHAR(MAX)     NULL,
        [CmpId]          NVARCHAR(50)      NULL,
        [UpdUser]        NVARCHAR(100)     NULL,
        [ClaimDate]      DATE              NOT NULL DEFAULT CAST(GETDATE() AS DATE),
        [CreatedAt]      DATETIME          NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]      DATETIME          NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_WarrantyClaim_CmpId ON [dbo].[WarrantyClaim]([CmpId]);
    CREATE INDEX IX_WarrantyClaim_ClaimDate ON [dbo].[WarrantyClaim]([ClaimDate]);

    PRINT 'Created table WarrantyClaim';
END
ELSE
    PRINT 'Table WarrantyClaim already exists';

-- ─── Seed: WarrantyDevice ────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[WarrantyDevice])
BEGIN
    INSERT INTO [dbo].[WarrantyDevice]
        ([SerialNo], [ProductName], [Brand], [Model], [Customer], [WarrantyStatus], [WarrantyExpiry])
    VALUES
        (N'FG60FT0001',  N'FortiGate 60F Next-Gen Firewall', N'Fortinet', N'FG-60F',        N'PTT Digital Solutions',  N'on',  '2027-10-04'),
        (N'FG100F0001',  N'FortiGate 100F',                  N'Fortinet', N'FG-100F',       N'SCG Cement Co.',         N'on',  '2027-06-30'),
        (N'FS124P0001',  N'FortiSwitch 124F-POE',            N'Fortinet', N'FS-124F-POE',   N'PTT Digital Solutions',  N'on',  '2027-10-04'),
        (N'FAP231F0001', N'FortiAP 231F',                    N'Fortinet', N'FAP-231F',      N'SCG Cement Co.',         N'on',  '2027-06-30'),
        (N'CS9300P0001', N'Cisco Catalyst 9300-24P',         N'Cisco',    N'C9300-24P',     N'Global Finance Group',   N'on',  '2026-12-31'),
        (N'CS9200T0001', N'Cisco Catalyst 9200-24T',         N'Cisco',    N'C9200-24T',     N'Siam Paragon Holdings',  N'off', '2023-10-12');

    PRINT 'Seeded WarrantyDevice (6 rows)';
END

-- ─── Seed: WarrantyClaim ─────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [dbo].[WarrantyClaim])
BEGIN
    INSERT INTO [dbo].[WarrantyClaim]
        ([Id], [TicketId], [Customer], [SalesName], [ReporterStaff],
         [Brand], [Model], [SerialNo], [WarrantyStatus], [Status], [Detail], [ClaimDate])
    VALUES
        (N'CLM-2023-0001', N'TK-0088', N'Global Finance Group', N'คุณสมศักดิ์ จันทร์ดี',
         N'นางสาวนกยูง สายทอง', N'Cisco', N'C9300-24P', N'CS9300P0001', N'on', N'Completed',
         N'อุปกรณ์ค้างหน้าบูตและบอร์ดชำรุด สลับอุปกรณ์เปลี่ยนเรียบร้อย', '2023-10-20');

    PRINT 'Seeded WarrantyClaim (1 row)';
END
