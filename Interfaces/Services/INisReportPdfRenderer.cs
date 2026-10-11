using goalongapi.Models.Nis;

namespace goalongapi.Services;

/// <summary>ผลการเรนเดอร์ PDF หนึ่งฉบับ — ไฟล์อยู่ที่ปลายทางแล้ว (เขียนไฟล์ชั่วคราวแล้ว rename · SPEC §9 ข้อ 31)</summary>
/// <param name="FilePath">path เต็มของไฟล์ PDF บนดิสก์</param>
/// <param name="Bytes">ขนาดไฟล์</param>
/// <param name="RenderMs">เวลาที่ใช้เรนเดอร์ (ms)</param>
public sealed record NisPdfRenderResult(string FilePath, long Bytes, int RenderMs);

/// <summary>
/// ตัวเรนเดอร์ PDF ของ NIS Service Report — ตัวจริงคือ Playwright/Chromium (`Services/NisReportPdfRenderer.cs`)
/// เปิด <c>{NisReport:CrmBaseUrl}/nsr-print/{reportId}?key={printKey}</c> ซึ่งหน้าพิมพ์ฝั่ง CRM เรียก
/// <c>GET api/nis/report/reports/{id}/print-data?key=</c> โดยไม่ต้องมี JWT (key คือสิทธิ์) — หน้าหนึ่งฉบับ
/// ถูกเปิดได้ถึง 3 ครั้ง (ปก / เนื้อ / ปกหลัง) key จึง**ไม่ถูกใช้ครั้งเดียวทิ้ง** แต่หมดอายุตาม PrintKeyTtlSeconds
///
/// สัญญา (SPEC §9 ข้อ 31 · 35 · 36): รอ <c>data-print-state="ready"</c> เท่านั้น · "error" = ล้มทันที ·
/// ห้าม waitForTimeout · เขียนไฟล์ชั่วคราวแล้ว rename · ตรวจฟอนต์ที่ฝังจริงด้วย pdffonts ทุกฉบับ
/// ลงทะเบียน AddSingleton ใน Program.cs เป็น <see cref="NisReportPdfRenderer"/> (ถือเบราว์เซอร์ตัวเดียวใช้ซ้ำ) ·
/// <see cref="NisReportPdfRendererUnavailable"/> คือตัวสำรองสลับกลับได้เมื่อเครื่องนั้นไม่มี Chromium · ติดตั้ง/Docker ดู docs/nsr-pdf-renderer.md
/// </summary>
public interface INisReportPdfRenderer
{
    /// <summary>
    /// เรนเดอร์รายงานหนึ่งฉบับเป็น PDF
    /// </summary>
    /// <param name="report">แถวรายงาน (มี LayoutSnapshotJson/DataSnapshotJson/CustomerId/PeriodStart ให้ประกอบหัว/ท้ายกระดาษและชื่อไฟล์)</param>
    /// <param name="printKey">print key ดิบที่เพิ่งออก (<c>NisReportService.IssuePrintKeyAsync</c>) — ใส่ใน query ?key=</param>
    /// <param name="onStage">รายงานขั้นที่งานผ่านจริง: "rendering" → "writing" (หน้าจอ poll เห็นสถานะจริง · SPEC §9 ข้อ 39)</param>
    /// <param name="ct">ยกเลิกเมื่อ host หยุด</param>
    /// <returns>path · ขนาด · เวลาที่ใช้</returns>
    /// <exception cref="InvalidOperationException">เรนเดอร์ไม่ได้ — ข้อความต้องอ่านออก (ถูกเก็บลง NisReportJob.Error)</exception>
    Task<NisPdfRenderResult> RenderAsync(NisReport report, string printKey, Action<string> onStage, CancellationToken ct);
}
