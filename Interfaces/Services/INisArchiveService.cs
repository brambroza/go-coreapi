using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;

namespace goalongapi.Services;

/// <summary>
/// อ่าน archive ของ NIS Service Report (NisServiceCase / NisSyncLog) — ชั้นที่ controller และ analytics เรียก
/// ทุกเมธอดรับ cmpId เป็น tenant key · ตัวกรองทั้งหมดผ่าน <see cref="BuildWhere"/> ตัวเดียว
/// </summary>
public interface INisArchiveService
{
    /// <summary>
    /// IQueryable ที่กรองตาม <see cref="NisCaseFilter"/> แล้ว (ยังไม่เรียง/แบ่งหน้า) — analytics ต้องใช้ตัวนี้
    /// ไม่คัดลอกเงื่อนไขไปเขียนซ้ำ (สองตัวเลขที่นับจากคนละชุดแถวคือบั๊ก ไม่ใช่ปรากฏการณ์ทางสถิติ)
    /// ผู้เรียกควร <c>Validate()</c> ตัวกรองก่อน
    /// </summary>
    IQueryable<NisServiceCase> BuildWhere(string cmpId, NisCaseFilter filter);

    /// <summary>รายการเคส + ตัวเลขสรุปของทั้งชุดที่กรอง · ล้าง resolutionHours ของกลุ่มลูกค้าบันทึกย้อนหลัง</summary>
    Task<NisCaseListResult> ListCasesAsync(string cmpId, NisCaseQuery query, CancellationToken ct = default);

    /// <summary>เคสเดียว — id ตรงกับ SourceId หรือ CaseNo หรือ ExternalGuid · null = ไม่พบ</summary>
    Task<NisServiceCaseDto?> GetCaseAsync(string cmpId, string id, CancellationToken ct = default);

    /// <summary>รายชื่อลูกค้า + จำนวนเคสแจ้งเข้าทั้งคลัง (ไม่รวมข้อมูลทดสอบ) เรียงมาก→น้อย</summary>
    Task<List<NisCustomerSummaryDto>> ListCustomersAsync(string cmpId, CancellationToken ct = default);

    /// <summary>เวลาที่ข้อมูล "ครบถึง" — รอบ ok ล่าสุดที่ InvalidCount=0 และ missing% ต่ำกว่าเกณฑ์ · null = ยังไม่มี</summary>
    Task<DateTimeOffset?> DataAsOfAsync(string cmpId, CancellationToken ct = default);

    /// <summary>ความสดแบบละเอียด — บอกได้ว่าทำไมตราเวลาไม่ขยับ</summary>
    Task<NisFreshnessDto> FreshnessAsync(string cmpId, CancellationToken ct = default);

    /// <summary>GET sync/status — ตัวเลข archive + สถานะ runtime ของ sync + 5 รอบล่าสุด</summary>
    Task<NisSyncStatusDto> SyncStatusAsync(string cmpId, CancellationToken ct = default);
}
