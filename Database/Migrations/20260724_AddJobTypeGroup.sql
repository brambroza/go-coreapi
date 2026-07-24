-- ============================================================
-- msb.mJobType — เพิ่มคอลัมน์ JobTypeGroup
-- ใช้แยกหมวดการใช้งานของ job type เช่น "service" สำหรับงานบริการ NIS
-- (แต่เดิม NIS เก็บ job type เป็น mock/pipe-string ใน dbo.NisSystemConfig
--  เปลี่ยนมาใช้ master table msb.mJobType จริงแทน)
-- Nullable — แถวเดิม (job type ของ quotation/purchasetracking) จะเป็น NULL
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- 1) เพิ่มคอลัมน์ (idempotent)
IF NOT EXISTS
(
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'msb.mJobType')
      AND name = N'JobTypeGroup'
)
BEGIN
    ALTER TABLE msb.mJobType ADD JobTypeGroup nvarchar(50) NULL;
END;

COMMIT TRANSACTION;
GO

-- 2) Backfill: mark job type ที่ตรงกับชุด service เดิมของ NIS ให้เป็น group='service'
--    (best-effort ต่อทุก CmpId ที่มี code เหล่านี้อยู่แล้ว)
UPDATE msb.mJobType
SET JobTypeGroup = N'service'
WHERE JobTypeGroup IS NULL
  AND JobTypeCode IN
  (
      N'Runrate', N'Implement',
      N'MA-Device', N'MA-Fortigate', N'MA-Software', N'MA-Network'
  );
GO

-- 3) Seed: ถ้าบริษัทไหนยังไม่มี service job type เลย ให้เติมชุดเริ่มต้นให้
--    (วน insert ต่อ CmpId ที่มีอยู่ใน master แต่ยังไม่มี group=service)
;WITH cmp AS
(
    SELECT DISTINCT CmpId FROM msb.mJobType
),
defaults(JobTypeCode, JobTypeName) AS
(
    SELECT N'Runrate',      N'Runrate'      UNION ALL
    SELECT N'Implement',    N'Implement'    UNION ALL
    SELECT N'MA-Device',    N'MA-Device'    UNION ALL
    SELECT N'MA-Fortigate', N'MA-Fortigate' UNION ALL
    SELECT N'MA-Software',  N'MA-Software'  UNION ALL
    SELECT N'MA-Network',   N'MA-Network'
)
INSERT INTO msb.mJobType
    (JobTypeCode, JobTypeName, JobTypeDescripton, JobTypeStateActive, JobTypeGroup, CmpId, UpdUser, UpdDate, UpdTime)
SELECT d.JobTypeCode, d.JobTypeName, N'', 1, N'service', c.CmpId, N'system',
       CAST(GETDATE() AS date), CAST(GETDATE() AS time)
FROM cmp c
CROSS JOIN defaults d
WHERE NOT EXISTS
(
    SELECT 1 FROM msb.mJobType j
    WHERE j.CmpId = c.CmpId AND j.JobTypeCode = d.JobTypeCode
);
GO

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   ALTER TABLE msb.mJobType DROP COLUMN JobTypeGroup;
--   (และถ้าต้องการล้าง seed: DELETE FROM msb.mJobType WHERE UpdUser=N'system' AND JobTypeGroup=N'service';)
-- ============================================================
