-- ============================================================
-- Migration: NIS Personal Todo + Note (แท็บบันทึกส่วนตัวของ staff · server-synced)
-- Feature: NIS-Backend — NisPersonalTodo + NisPersonalNote
-- Date: 2026-07-16
-- Author: Claude
-- ============================================================
-- ⚠️ ต้องขออนุมัติก่อนรันบน production DB (goalongdatabase_production)
-- Safe to re-run: ทุก block guarded ด้วย IF NOT EXISTS · Rollback อยู่ท้ายไฟล์
--
-- owner ต่อแถว = (CmpId, AccountId) · Id = client-generated NVARCHAR PK
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisPersonalTodo')
BEGIN
    CREATE TABLE [dbo].[NisPersonalTodo] (
        [Id]             NVARCHAR(60)   NOT NULL PRIMARY KEY,   -- client gen เช่น 'todo-1720000000000'
        [CmpId]          NVARCHAR(50)   NOT NULL,
        [AccountId]      BIGINT         NOT NULL,
        [Text]           NVARCHAR(MAX)  NOT NULL,
        [RemindDateTime] NVARCHAR(30)   NULL,                   -- 'YYYY-MM-DDTHH:MM' หรือ NULL
        [Done]           BIT            NOT NULL DEFAULT 0,
        [CreatedDate]    DATETIME       NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedDate]    DATETIME       NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_NisPersonalTodo_Owner] ON [dbo].[NisPersonalTodo] ([CmpId], [AccountId]);
    PRINT 'Created table NisPersonalTodo';
END
ELSE
    PRINT 'Table NisPersonalTodo already exists';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NisPersonalNote')
BEGIN
    CREATE TABLE [dbo].[NisPersonalNote] (
        [Id]          NVARCHAR(60)   NOT NULL PRIMARY KEY,       -- client gen เช่น 'note-1720000000000'
        [CmpId]       NVARCHAR(50)   NOT NULL,
        [AccountId]   BIGINT         NOT NULL,
        [Text]        NVARCHAR(MAX)  NOT NULL,
        [Reminder]    NVARCHAR(30)   NULL,                       -- 'YYYY-MM-DDTHH:MM' หรือ NULL
        [CreatedDate] DATETIME       NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX [IX_NisPersonalNote_Owner] ON [dbo].[NisPersonalNote] ([CmpId], [AccountId]);
    PRINT 'Created table NisPersonalNote';
END
ELSE
    PRINT 'Table NisPersonalNote already exists';
GO

-- ============================================================
-- ROLLBACK (รันเมื่อต้องการถอน):
--   DROP TABLE IF EXISTS [dbo].[NisPersonalTodo];
--   DROP TABLE IF EXISTS [dbo].[NisPersonalNote];
-- ============================================================
