-- ============================================================
-- NIS Project — Attachment metadata (เอกสารแนบตอนสร้างโครงการ)
-- เก็บ metadata ของไฟล์ที่แนบตอนเปิด Project (PDF/Excel/Word/Visio/Image)
-- ตัวไฟล์จริงเก็บบนดิสก์ผ่าน endpoint กลาง /uploadallfile + /movefile
-- ตารางนี้เก็บแค่ FileName + FilePath + Seq เพื่อให้หน้า list แสดง/ดาวน์โหลดได้
-- FK -> dbo.NisProject (ProjectId) ON DELETE CASCADE (ลบ project → ลบ metadata ไฟล์)
-- ============================================================
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.NisProjectFile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NisProjectFile
    (
        FileId nvarchar(50) NOT NULL CONSTRAINT PK_NisProjectFile PRIMARY KEY,
        ProjectId nvarchar(50) NOT NULL,
        -- ชื่อไฟล์เดิมที่ผู้ใช้เห็น เช่น "Network-Diagram.pdf"
        FileName nvarchar(300) NOT NULL,
        -- URL/path เต็มหลัง movefile เช่น "{serverUrl}/{cmpId}/nis/{projectNo}/{fileName}"
        FilePath nvarchar(1000) NOT NULL,
        -- ลำดับการแสดงผล (1-based)
        Seq int NOT NULL CONSTRAINT DF_NisProjectFile_Seq DEFAULT (1),
        FileSize bigint NOT NULL CONSTRAINT DF_NisProjectFile_FileSize DEFAULT (0),
        CmpId nvarchar(50) NULL,
        CreatedBy nvarchar(100) NULL,
        CreatedDate datetime NOT NULL CONSTRAINT DF_NisProjectFile_CreatedDate DEFAULT (GETDATE()),
        CONSTRAINT FK_NisProjectFile_NisProject FOREIGN KEY (ProjectId)
            REFERENCES dbo.NisProject (ProjectId) ON DELETE CASCADE
    );
END;

-- index ช่วยดึงไฟล์ต่อ project (Include(p => p.Files))
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.NisProjectFile')
      AND name = N'IX_NisProjectFile_ProjectId'
)
BEGIN
    EXEC sys.sp_executesql N'
        CREATE INDEX IX_NisProjectFile_ProjectId
            ON dbo.NisProjectFile (ProjectId);';
END;

COMMIT TRANSACTION;

-- ============================================================
-- Rollback (manual — ถ้าต้องถอน):
--   DROP TABLE IF EXISTS dbo.NisProjectFile;
-- ============================================================
