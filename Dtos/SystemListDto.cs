namespace goalongapi.Dtos
{
    /// <summary>
    /// รายการค่าใน master แบบ list (ตาราง dbo.SystemList) เช่น ListName = "Warranty", "ProdVatType"
    /// ใช้กับ endpoint setsystemlistdata / deletesystemlistdata
    /// </summary>
    public class SystemListDto
    {
        /// <summary>Id ของรายการภายใน ListName + CmpId เดียวกัน — ส่ง 0 หรือค่าติดลบ = เพิ่มรายการใหม่</summary>
        public int Id { get; set; }

        /// <summary>ชื่อชุดรายการ เช่น "Warranty"</summary>
        public string ListName { get; set; }

        /// <summary>ข้อความที่แสดงให้ผู้ใช้เลือก เช่น "รับประกัน 1 ปี"</summary>
        public string ListDescription { get; set; }

        /// <summary>รหัสบริษัท (tenant key)</summary>
        public string CmpId { get; set; }

        /// <summary>ผู้ทำรายการ (เก็บไว้เพื่อ log เท่านั้น ตาราง SystemList ไม่มีคอลัมน์นี้)</summary>
        public string UpdUser { get; set; }
    }
}
