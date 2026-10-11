using goalongapi.Models.Nis;

namespace goalongapi.Services;

/// <summary>
/// ตัวเรนเดอร์สำรองเมื่อยังไม่ได้ติดตั้ง Playwright/Chromium บนเครื่องนี้ — โยนข้อความภาษาคนทันที
/// ให้ <see cref="NisReportPdfQueueService"/> ปล่อยงานล้มพร้อมเหตุผลที่อ่านออก (SPEC §9 ข้อ 34) แทนที่จะค้างเงียบ
/// ปัจจุบัน Program.cs ลงทะเบียนตัวจริง <see cref="NisReportPdfRenderer"/> (AddSingleton) แล้ว — คลาสนี้เก็บไว้สลับกลับ
/// ด้วยการเปลี่ยนบรรทัดนั้นบรรทัดเดียว เมื่อ environment ไหนยังไม่มี Chromium
/// </summary>
public sealed class NisReportPdfRendererUnavailable : INisReportPdfRenderer
{
    /// <inheritdoc />
    public Task<NisPdfRenderResult> RenderAsync(NisReport report, string printKey, Action<string> onStage, CancellationToken ct) =>
        throw new InvalidOperationException("ตัวเรนเดอร์ PDF ยังไม่ติดตั้งบนเครื่องนี้ — ติดต่อผู้ดูแลระบบ");
}
