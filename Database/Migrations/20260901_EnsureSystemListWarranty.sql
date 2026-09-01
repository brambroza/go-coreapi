-- ============================================================
-- Migration: master "การรับประกัน" (Warranty) บนตาราง dbo.SystemList
--
--   1) ตรวจให้แน่ใจว่า SystemList มีคอลัมน์ CmpId / StateActive
--      (endpoint ใหม่ setsystemlistdata / deletesystemlistdata scope ด้วย CmpId เสมอ)
--   2) seed ค่าเริ่มต้นของ ListName = 'Warranty' ให้ทุก CmpId ที่มีอยู่แล้วใน SystemList
--
--   หมายเหตุ: ตาราง SystemList เป็นของเดิม ไม่มี DDL อยู่ใน repo
--   ถ้าคอลัมน์ CmpId เพิ่งถูกเพิ่มโดยสคริปต์นี้ (ของเดิมไม่มี) ต้องตรวจ SP dbo.getlistdata ด้วยว่า
--   กรอง @cmpid อย่างไร แถวเก่าที่ CmpId เป็น NULL จะไม่ถูกกรองตามบริษัท
--
-- Idempotent: รันซ้ำได้
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SystemList', N'U') IS NOT NULL
BEGIN
    -- 1) คอลัมน์ที่จำเป็น -------------------------------------------------
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.SystemList')
          AND name = N'CmpId'
    )
    BEGIN
        ALTER TABLE dbo.SystemList ADD CmpId nvarchar(50) NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.SystemList')
          AND name = N'StateActive'
    )
    BEGIN
        ALTER TABLE dbo.SystemList ADD StateActive nvarchar(1) NULL;
    END;
END;

COMMIT TRANSACTION;
GO

-- 2) seed ค่าเริ่มต้นของ Warranty ต่อ CmpId --------------------------------
-- แยก batch ด้วย GO เพื่อให้คอลัมน์ CmpId ที่เพิ่งเพิ่มถูกมองเห็นตอน compile
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SystemList', N'U') IS NOT NULL
BEGIN
    DECLARE @DefaultWarranty TABLE (Seq int, ListDescription nvarchar(200));

    INSERT INTO @DefaultWarranty (Seq, ListDescription)
    VALUES (1, N'ไม่มีการรับประกัน'),
           (2, N'รับประกัน 3 เดือน'),
           (3, N'รับประกัน 6 เดือน'),
           (4, N'รับประกัน 1 ปี'),
           (5, N'รับประกัน 2 ปี'),
           (6, N'รับประกัน 3 ปี');

    -- บริษัทที่มีข้อมูลอยู่แล้วใน SystemList
    DECLARE @Cmp TABLE (CmpId nvarchar(50));

    INSERT INTO @Cmp (CmpId)
    SELECT DISTINCT CmpId
      FROM dbo.SystemList
     WHERE CmpId IS NOT NULL
       AND LTRIM(RTRIM(CmpId)) <> '';

    INSERT INTO dbo.SystemList (Id, ListName, ListDescription, StateActive, CmpId)
    SELECT d.Seq, N'Warranty', d.ListDescription, N'1', c.CmpId
      FROM @DefaultWarranty d
     CROSS JOIN @Cmp c
     WHERE NOT EXISTS
    (
        SELECT 1
          FROM dbo.SystemList s
         WHERE s.ListName = N'Warranty'
           AND s.CmpId = c.CmpId
           AND s.ListDescription = d.ListDescription
    );
END;

COMMIT TRANSACTION;

-- ============================================================
-- ตรวจผล:
--   SELECT * FROM dbo.SystemList WHERE ListName = N'Warranty' ORDER BY CmpId, Id;
--
-- Rollback (manual):
--   DELETE FROM dbo.SystemList WHERE ListName = N'Warranty';
--   -- คอลัมน์ CmpId / StateActive ห้ามลบถ้ามีข้อมูลอื่นใช้อยู่แล้ว
-- ============================================================
