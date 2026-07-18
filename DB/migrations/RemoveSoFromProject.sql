-- ============================================================================
-- Stored Procedure: dbo.RemoveSoFromProject
-- Purpose : ถอด 1 Sale Order ออกจาก Project (unlink) — inverse ของ dbo.SetProjectAppByPO
-- Called by: ProjectController.removeSoFromProject  (POST /removeSoFromProject)
-- Params  : @UpdUser, @ProjectNo, @SaleOrderNo, @CmpId
--
-- สิ่งที่ต้องทำ (2 ขั้น):
--   1) คืนสถานะ Sale Order กลับเป็น 'statuswaitprojectcreate' และล้างการอ้างอิง Project
--      ออกจาก Sale Order (เป็น inverse ของสิ่งที่ SetProjectAppByPO ทำตอน link)
--   2) ลบรายการ Project Detail (items) ที่มาจาก Sale Order นี้ ภายใต้ Project นี้
--
-- ⚠️ DBA: ชื่อ table/column ด้านล่างเป็น "โครงตัวอย่าง" — โปรดยืนยันกับสคีมาจริง
--    โดยเทียบกับ body ของ dbo.SetProjectAppByPO (ตัว link) แล้วทำ inverse ให้ตรงกัน
--    อย่ารันบน production ก่อนตรวจชื่อ table/column และทดสอบบน staging
-- ============================================================================

CREATE OR ALTER PROCEDURE dbo.RemoveSoFromProject
    @UpdUser     NVARCHAR(100) = NULL,
    @ProjectNo   NVARCHAR(50),
    @SaleOrderNo NVARCHAR(50),
    @CmpId       NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1) คืนสถานะ Sale Order กลับ "รอเปิดโปรเจค" + ล้าง ProjectNo ที่ผูกไว้
        --    TODO(DBA): แก้ชื่อ table/column ให้ตรงกับที่ SetProjectAppByPO ใช้ตอน link
        UPDATE SO
        SET
            SO.SaleOrderState    = 'statuswaitprojectcreate',
            SO.StateCreateProject = 0,
            SO.ProjectNo         = ''            -- ถ้ามีคอลัมน์อ้างอิง Project บน Sale Order
        FROM /* TODO: dbo.<SaleOrderHeaderTable> */ dbo.SaleOrderH AS SO
        WHERE SO.SaleOrderNo = @SaleOrderNo
          AND (@CmpId IS NULL OR SO.CmpId = @CmpId);

        -- 2) ลบ Project Detail ที่มาจาก Sale Order นี้ ภายใต้ Project นี้
        --    TODO(DBA): ยืนยันชื่อ table detail (ที่ SetProject_Detail เขียนลง)
        DELETE PD
        FROM /* TODO: dbo.<ProjectDetailTable> */ dbo.Project_Detail AS PD
        WHERE PD.ProjectNo   = @ProjectNo
          AND PD.SaleOrderNo = @SaleOrderNo
          AND (@CmpId IS NULL OR PD.CmpId = @CmpId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
