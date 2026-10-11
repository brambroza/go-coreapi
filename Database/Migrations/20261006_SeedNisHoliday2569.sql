-- ============================================================
-- Migration: NIS Service Report — seed วันหยุดราชการ พ.ศ. 2569 (ค.ศ. 2026) ลง dbo.NisHoliday
--   ที่มา: subrepo-service/apps/api/prisma/holidays.json (24 วัน)
--
-- ความเป็นเจ้าของ (ตาม seed-holidays.ts เดิม):
--   • seed เป็นเจ้าของ "ปฏิทินราชการ" — NameTh / Verified / Note ของแถว Kind='government'
--   • คนเป็นเจ้าของ Observed (NIS ปิดจริงไหม) และแถว Kind='company' — seed ไม่แตะสองอย่างนี้เด็ดขาด
--     ต่อให้รันซ้ำกี่รอบ (MERGE: WHEN NOT MATCHED → INSERT Observed=0 · WHEN MATCHED เฉพาะ Kind='government'
--     → UPDATE NameTh/Verified/Note เท่านั้น)
--
-- ต้องรัน 20261006_AddNisHoliday.sql ก่อน · Idempotent: รันซ้ำได้
-- ⚠️ ยังไม่ได้รันบนฐานใด — รันมือบน staging/production หลังได้ approval
-- ============================================================
SET XACT_ABORT ON;
SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.NisHoliday', N'U') IS NULL
    THROW 50000, N'ต้องรัน 20261006_AddNisHoliday.sql ก่อน (ไม่พบตาราง dbo.NisHoliday)', 1;

BEGIN TRANSACTION;

-- บริษัทที่เปิดใช้ NIS Service Report — ต้องตรงกับ appsettings NisReport:CmpIds
-- เพิ่มบริษัทใหม่: เพิ่มบรรทัด VALUES แล้วรันไฟล์นี้ซ้ำ (แถวเดิมไม่ถูกแตะ)
DECLARE @CmpIds TABLE (CmpId nvarchar(50) NOT NULL PRIMARY KEY);
INSERT INTO @CmpIds (CmpId) VALUES
    (N'230015');

DECLARE @Seed TABLE
(
    [Date]   nvarchar(10)  NOT NULL PRIMARY KEY,
    NameTh   nvarchar(200) NOT NULL,
    Verified bit           NOT NULL,
    Note     nvarchar(500) NULL
);
INSERT INTO @Seed ([Date], NameTh, Verified, Note) VALUES
    (N'2026-01-01', N'วันขึ้นปีใหม่', 1, NULL),
    (N'2026-01-02', N'วันหยุดพิเศษ (มติ ครม.)', 1, NULL),
    (N'2026-03-03', N'วันมาฆบูชา', 1, NULL),
    (N'2026-04-06', N'วันจักรี', 1, NULL),
    (N'2026-04-13', N'วันสงกรานต์', 1, NULL),
    (N'2026-04-14', N'วันสงกรานต์', 1, NULL),
    (N'2026-04-15', N'วันสงกรานต์', 1, NULL),
    (N'2026-05-01', N'วันแรงงานแห่งชาติ', 1, NULL),
    (N'2026-05-04', N'วันฉัตรมงคล', 1, NULL),
    (N'2026-05-13', N'วันพืชมงคล', 1, N'ราชการหยุด ธนาคารไม่หยุด — เอกชนส่วนใหญ่ไม่หยุด ต้องยืนยันกับ HR ก่อนตั้ง observed'),
    (N'2026-05-31', N'วันวิสาขบูชา', 1, N'ตรงกับวันอาทิตย์'),
    (N'2026-06-01', N'ชดเชยวันวิสาขบูชา', 1, NULL),
    (N'2026-06-02', N'วันหยุดต่อเนื่อง 1–3 มิ.ย. (ยังไม่ยืนยัน)', 0, N'บางแหล่งมี บางแหล่งไม่มี — ห้ามนับเป็นวันหยุดจนกว่าจะยืนยันได้'),
    (N'2026-06-03', N'วันเฉลิมพระชนมพรรษาสมเด็จพระนางเจ้าฯ พระบรมราชินี', 1, NULL),
    (N'2026-07-28', N'วันเฉลิมพระชนมพรรษาพระบาทสมเด็จพระเจ้าอยู่หัว', 1, NULL),
    (N'2026-07-29', N'วันอาสาฬหบูชา', 1, NULL),
    (N'2026-07-30', N'วันเข้าพรรษา', 1, NULL),
    (N'2026-08-12', N'วันเฉลิมพระชนมพรรษาสมเด็จพระบรมราชชนนีพันปีหลวง (วันแม่แห่งชาติ)', 1, NULL),
    (N'2026-10-13', N'วันคล้ายวันสวรรคตพระบาทสมเด็จพระบรมชนกาธิเบศร (ร.9)', 1, NULL),
    (N'2026-10-23', N'วันปิยมหาราช', 1, NULL),
    (N'2026-12-05', N'วันคล้ายวันพระบรมราชสมภพ ร.9 (วันพ่อแห่งชาติ)', 1, N'ตรงกับวันเสาร์'),
    (N'2026-12-07', N'ชดเชยวันพ่อแห่งชาติ', 1, NULL),
    (N'2026-12-10', N'วันรัฐธรรมนูญ', 1, NULL),
    (N'2026-12-31', N'วันสิ้นปี', 1, NULL);

MERGE dbo.NisHoliday AS t
USING
(
    SELECT c.CmpId,
           s.[Date],
           CAST(LEFT(s.[Date], 4) AS int) AS [Year],
           s.NameTh,
           s.Verified,
           s.Note
    FROM @Seed s
    CROSS JOIN @CmpIds c
) AS src
    ON t.CmpId = src.CmpId AND t.[Date] = src.[Date]
WHEN MATCHED AND t.Kind = N'government'
             AND (t.NameTh <> src.NameTh
                  OR t.Verified <> src.Verified
                  OR ISNULL(t.Note, N'') <> ISNULL(src.Note, N''))
    THEN UPDATE SET
        t.NameTh   = src.NameTh,
        t.Verified = src.Verified,
        t.Note     = src.Note
        -- ไม่แตะ Observed · ไม่แตะแถว Kind='company'
WHEN NOT MATCHED BY TARGET
    THEN INSERT (CmpId, [Date], [Year], NameTh, Kind, Verified, Observed, Note)
         VALUES (src.CmpId, src.[Date], src.[Year], src.NameTh, N'government', src.Verified, 0, src.Note);

PRINT CONCAT(N'seed NisHoliday 2569: ', @@ROWCOUNT, N' แถวถูกเพิ่ม/อัปเดต');

COMMIT TRANSACTION;

-- ============================================================
-- หลังรัน: ทุกแถวยังเป็น Observed=0 — HR/คุณ Tone ต้องยืนยันทีละวันผ่านหน้าจอ (PUT api/nis/archive/holidays/{date})
-- ตราบใดที่ปียังยืนยันไม่ครบ ตัวเลขที่อิงเวลาทำการของปีนั้นจะไม่แสดง (SPEC §9 ข้อ 67 — ตั้งใจ)
-- Rollback (manual): DELETE FROM dbo.NisHoliday WHERE Kind = N'government' AND [Year] = 2026 AND Observed = 0;
-- ============================================================
