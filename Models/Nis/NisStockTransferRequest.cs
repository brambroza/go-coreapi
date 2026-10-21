using System.ComponentModel.DataAnnotations;

namespace goalongapi.Models.Nis;

// ── NisStockTransferRequest ─────────────────────────────────────────────────────
// คำขอโยกย้ายคลัง (dbo.NisStockTransferRequest) — ช่างยื่นจากหน้าเบิกของ (onsite) เมื่อคลัง 2
// Spare ลูกค้า / คลัง 3 ของใน Stock ไม่มีของที่ต้องใช้ SM เป็นผู้โยกย้ายจริงผ่าน WMS โอนย้ายคลัง
// (transferwh) แล้วกด "โยกแล้ว" ที่นี่ — ตารางนี้เป็นแค่ใบแจ้ง ไม่ขยับสต็อกเอง

public class NisStockTransferRequest
{
    [Key]
    [MaxLength(50)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(50)]
    public string CmpId { get; set; } = string.Empty;

    /// NisTicket.TicketId ของงานที่ต้องใช้ของ (optional — ขอจากหน้าคลังได้โดยไม่ผูกตั๋ว)
    [MaxLength(50)]
    public string? TicketId { get; set; }

    /// รหัสสินค้า WMS (ProductCode)
    [MaxLength(100)]
    public string ProdCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ProdName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string UnitCode { get; set; } = string.Empty;

    public decimal Qty { get; set; }

    /// mWareHouse.WareHouseId ปลายทาง — ต้องเป็นคลัง Spare ลูกค้า หรือ ของใน Stock ตาม System Config
    public int ToWhId { get; set; }

    /// รายละเอียดถึง SM
    [MaxLength(1000)]
    public string Note { get; set; } = string.Empty;

    /// ช่าง (username หรือ FullName) ผู้ยื่นคำขอ
    [MaxLength(200)]
    public string RequestedBy { get; set; } = string.Empty;

    /// Pending | Done | Rejected
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// SM ผู้โยกย้าย / ปฏิเสธ
    [MaxLength(200)]
    public string? HandledBy { get; set; }

    /// เลขใบโอนย้ายคลังใน WMS (TRW-…) — optional
    [MaxLength(50)]
    public string? TransferDocNo { get; set; }

    [MaxLength(500)]
    public string? RejectReason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;
}
