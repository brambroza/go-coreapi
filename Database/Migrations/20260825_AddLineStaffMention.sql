-- ============================================================
-- 20260825_AddLineStaffMention.sql
-- ตาราง mapping ระหว่างสมาชิกกลุ่ม LINE (staff) กับชื่อผู้ดูแลเคส
-- ใช้โดย go-chat-api เพื่อ @mention ผู้ดูแลเคสตอนแจ้งเตือนเข้ากลุ่ม staff
--
-- go-chat-api เขียน LineGroupId / LineUserId / LineDisplayName ให้อัตโนมัติ
-- จาก webhook เมื่อสมาชิกพิมพ์ข้อความในกลุ่ม
-- ส่วน AssignName ต้อง map มือครั้งเดียวต่อคน (ดู UPDATE ตัวอย่างท้ายไฟล์)
-- ถ้ายังไม่ map -> ไม่มี mention -> ระบบส่ง Flex เหมือนเดิม
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'LineStaffMention' AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.LineStaffMention (
        Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CmpId           VARCHAR(30)   NOT NULL,
        LineGroupId     VARCHAR(100)  NOT NULL,   -- = userIds ที่ dbo.getServiceTeam คืน
        LineUserId      VARCHAR(100)  NOT NULL,
        LineDisplayName NVARCHAR(200) NULL,       -- ชื่อใน LINE (เติมอัตโนมัติจาก webhook)
        AssignName      NVARCHAR(200) NULL,       -- ต้องตรงกับ assignname ของ dbo.getServiceTeam
        IsActive        BIT NOT NULL CONSTRAINT DF_LineStaffMention_IsActive DEFAULT (1),
        UpdatedAt       DATETIME NOT NULL CONSTRAINT DF_LineStaffMention_UpdatedAt DEFAULT (GETDATE())
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_LineStaffMention_Group_User' AND object_id = OBJECT_ID('dbo.LineStaffMention')
)
BEGIN
    CREATE UNIQUE INDEX UX_LineStaffMention_Group_User
        ON dbo.LineStaffMention (LineGroupId, LineUserId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_LineStaffMention_Assign' AND object_id = OBJECT_ID('dbo.LineStaffMention')
)
BEGIN
    CREATE INDEX IX_LineStaffMention_Assign
        ON dbo.LineStaffMention (LineGroupId, AssignName)
        WHERE IsActive = 1;
END
GO

-- ------------------------------------------------------------
-- ขั้นตอน map ครั้งแรก (ทำหลังจากสมาชิกพิมพ์ในกลุ่มแล้วอย่างน้อย 1 ครั้ง)
--
-- 1) ดูรายชื่อที่ระบบเก็บมาได้
--    SELECT LineGroupId, LineUserId, LineDisplayName, AssignName
--    FROM dbo.LineStaffMention ORDER BY UpdatedAt DESC;
--
-- 2) เติม AssignName ให้ตรงกับค่า assignname ที่ dbo.getServiceTeam คืน
--    UPDATE dbo.LineStaffMention
--    SET AssignName = N'<ชื่อผู้ดูแลเคสตามระบบ>'
--    WHERE LineGroupId = '<Cxxxxxxxx>' AND LineUserId = '<Uxxxxxxxx>';
-- ------------------------------------------------------------
