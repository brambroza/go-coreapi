using System.Data;
using goalongapi.Data;
using goalongapi.DB;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace goalongapi.Services;

/// <summary>
/// ข้อมูลบริษัทที่พิมพ์ลงเอกสาร (SPEC §9 ข้อ 27) — port จาก apps/api/src/reports/company.service.ts
///
/// 🔴 **แหล่งข้อมูลสองชั้น — ไม่กรอกข้อมูลบริษัทซ้ำ**
///   1. ช่องบริษัท (ชื่อ/ที่อยู่/โทร/อีเมล/เว็บ/เลขภาษี/LINE ID) อ่านจาก <b>ข้อมูลบริษัทของระบบหลัก</b>
///      (SP <c>dbo.getcmpinfo</c> — ชุดเดียวกับหน้า ตั้งค่า › ข้อมูลบริษัท ของ CRM และใบเสนอราคา/ใบส่งของ)
///      แก้ที่หน้านั้นเท่านั้น · PUT ของโมดูลนี้ปฏิเสธช่องเหล่านี้ (400) — ไม่ให้มีค่าสองชุดที่เพี้ยนออกจากกัน
///   2. ส่วนที่ระบบหลักไม่มี (<c>lineQrPath</c> · <c>salesContacts</c>) เก็บใน <see cref="NisReportCompany"/>.InfoJson
///   SP อ่านไม่ได้ (ฐานเก่า/ผิดพลาด) → ใช้ค่าช่องบริษัทที่ค้างใน InfoJson (ข้อมูลที่ import มาจาก NSR เดิม) แทน
///   และบอกผ่าน <c>masterAvailable=false</c> — ไม่ทำให้ออกรายงานล้ม
///
/// 🔴 ยังไม่มีแถว = ทุกช่องว่าง ไม่ใช่ error — ระบบต้องออกรายงานได้ตอนที่ยังไม่มีใครกรอก (ไม่บล็อก export)
/// 🔴 เขียน = **รวมกับของเดิม ไม่ใช่เขียนทับทั้งก้อน** — ยิง {"lineQrPath":"x"} มาแล้ว salesContacts ต้องไม่ถูกล้างเงียบ ๆ
/// snapshot ของรายงานแช่แข็งก้อนที่รวมแล้ว (<see cref="ReadAsync"/>) — เอกสารเก่าไม่เปลี่ยนตามเมื่อแก้ข้อมูลบริษัททีหลัง
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface จึงไม่ถูก Autofac auto-register)
/// </summary>
public class NisReportCompanyService
{
    private static readonly JsonSerializerSettings CamelCase = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Include,
    };

    /// <summary>ช่องที่มาจากข้อมูลบริษัทของระบบหลัก (camelCase ตรงกับ CompanyInfoSchema) — PUT ของโมดูลนี้ห้ามแก้</summary>
    public static readonly IReadOnlyList<string> MasterFields = new[]
    {
        "nameTh", "nameEn", "addressTh", "addressEn", "tel", "email", "website", "taxId", "lineId",
    };

    private readonly DatabaseContext _db;
    private readonly DbConnectionFactory _factory;
    private readonly ILogger<NisReportCompanyService> _logger;

    public NisReportCompanyService(DatabaseContext db, DbConnectionFactory factory, ILogger<NisReportCompanyService> logger)
    {
        _db = db;
        _factory = factory;
        _logger = logger;
    }

    /// <summary>
    /// ข้อมูลบริษัทของบริษัท (tenant) นี้ — ช่องบริษัทจากระบบหลัก + QR/เซลส์จาก InfoJson
    /// ไม่มีแถวและ SP ไม่มีข้อมูล = ทุกช่องว่าง (EMPTY_COMPANY)
    /// </summary>
    public async Task<NisCompanyInfoDto> ReadAsync(string cmpId, CancellationToken ct = default) =>
        (await ReadMergedAsync(cmpId, ct)).Dto;

    /// <summary>GET company — ข้อมูล + ช่องที่ยังว่างซึ่งต้องกรอกก่อนขึ้น production + ช่องที่แก้ได้เฉพาะหน้าข้อมูลบริษัทของระบบหลัก</summary>
    public async Task<NisCompanyInfoResponseDto> ReadWithGapsAsync(string cmpId, CancellationToken ct = default)
    {
        var (dto, masterAvailable) = await ReadMergedAsync(cmpId, ct);
        return WithGaps(dto, masterAvailable);
    }

    /// <summary>
    /// PUT company — merge patch เข้ากับของเดิม (ระดับบนสุด เหมือน <c>{...read(), ...patch}</c>) แล้วตรวจทั้งก้อน
    /// รับเฉพาะช่องของโมดูลนี้ (<c>lineQrPath</c> · <c>salesContacts</c>) — ช่องบริษัทส่งมา → 400 ชี้ไปหน้าข้อมูลบริษัท
    /// ลูกค้าหนึ่งรายผูกกับเซลส์ได้คนเดียว (คุณ Tone 18 ก.ย. 2569) — ซ้อนกัน → 400
    /// </summary>
    /// <exception cref="NisReportBadRequestException">รูปไม่ถูกต้อง · ส่งช่องบริษัทของระบบหลัก · หรือลูกค้าถูกผูกกับเซลส์มากกว่าหนึ่งคน</exception>
    public async Task<NisCompanyInfoResponseDto> WriteAsync(string cmpId, JToken? patch, string updatedBy, CancellationToken ct = default)
    {
        var row = await _db.NisReportCompanies.FirstOrDefaultAsync(c => c.CmpId == cmpId, ct);
        var current = NisReportJson.TryParseObject(row?.InfoJson) ?? new JObject();
        var merged = (JObject)current.DeepClone();
        if (patch is JObject po)
        {
            var locked = po.Properties().Select(x => x.Name).Where(n => MasterFields.Contains(n)).ToList();
            if (locked.Count > 0)
                throw new NisReportBadRequestException(
                    $"ช่อง {string.Join(" / ", locked)} แก้ที่ ตั้งค่า › ข้อมูลบริษัท ของระบบหลัก (ไม่เก็บซ้ำในโมดูลรายงาน)",
                    new Dictionary<string, object?> { ["masterFields"] = locked });
            foreach (var prop in po.Properties()) merged[prop.Name] = prop.Value.DeepClone();
        }
        else if (patch != null && patch.Type != JTokenType.Null)
        {
            throw new NisReportBadRequestException("ข้อมูลที่ส่งมาไม่ถูกต้อง", new Dictionary<string, object?> { ["issues"] = new[] { "(ราก): ต้องเป็น JSON object" } });
        }

        var next = Parse(merged, out var issues);
        if (issues.Count > 0)
            throw new NisReportBadRequestException("ข้อมูลที่ส่งมาไม่ถูกต้อง", new Dictionary<string, object?> { ["issues"] = issues });

        var clash = NisReportSchema.CustomerOwnerConflicts(next.SalesContacts);
        if (clash.Count > 0)
            throw new NisReportBadRequestException(
                $"ลูกค้า {string.Join(" / ", clash.Select(c => $"{c.CustomerId} ({string.Join(" · ", c.Names)})"))} ถูกผูกกับเซลส์มากกว่าหนึ่งคน — ติ๊กไว้ที่คนเดียว");

        var json = Serialize(next);
        if (row == null)
        {
            row = new NisReportCompany { CmpId = cmpId };
            _db.NisReportCompanies.Add(row);
        }
        row.InfoJson = json;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync(ct);

        // ตอบกลับด้วยก้อนที่รวมกับระบบหลักแล้ว — หน้าจอต้องเห็นชุดเดียวกับที่ GET คืน
        var master = await ReadMasterAsync(cmpId, ct);
        return WithGaps(Overlay(master, next), master != null);
    }

    /// <summary>ช่องบริษัทจากระบบหลักทับลงบน InfoJson — ระบบหลักอ่านไม่ได้ (null) = ใช้ค่าใน InfoJson ตามเดิม</summary>
    private async Task<(NisCompanyInfoDto Dto, bool MasterAvailable)> ReadMergedAsync(string cmpId, CancellationToken ct)
    {
        var row = await _db.NisReportCompanies.AsNoTracking().FirstOrDefaultAsync(c => c.CmpId == cmpId, ct);
        var stored = Parse(NisReportJson.TryParseObject(row?.InfoJson), out var issues) is { } dto && issues.Count == 0 ? dto : new NisCompanyInfoDto();
        var master = await ReadMasterAsync(cmpId, ct);
        return (Overlay(master, stored), master != null);
    }

    private static NisCompanyInfoDto Overlay(NisCompanyInfoDto? master, NisCompanyInfoDto stored)
    {
        if (master == null) return stored;
        return new NisCompanyInfoDto
        {
            NameTh = master.NameTh, NameEn = master.NameEn, AddressTh = master.AddressTh, AddressEn = master.AddressEn,
            Tel = master.Tel, Email = master.Email, Website = master.Website, TaxId = master.TaxId, LineId = master.LineId,
            LineQrPath = stored.LineQrPath,
            SalesContacts = stored.SalesContacts,
        };
    }

    /// <summary>
    /// อ่านข้อมูลบริษัทของระบบหลัก (SP <c>dbo.getcmpinfo @CmpId</c> — parameterized ไม่ต่อสตริง · ไม่ใช้ DBConn static)
    /// คอลัมน์ต่างชุดกันตามเวอร์ชันฐาน → อ่านแบบทนคอลัมน์หาย · โทรใช้ Phone ก่อน ว่างค่อย TelOffice (ตรรกะเดียวกับลายเซ็นอีเมล NIS)
    /// ไม่มีแถว / SP ผิดพลาด → null (ผู้เรียก fallback ไป InfoJson) — ห้ามทำให้ออกรายงานล้ม
    /// </summary>
    private async Task<NisCompanyInfoDto?> ReadMasterAsync(string cmpId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmpId)) return null;
        try
        {
            await using var conn = _factory.CreateConnection();
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand("dbo.getcmpinfo", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add(new SqlParameter("@CmpId", SqlDbType.NVarChar, 50) { Value = cmpId });
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++) columns.Add(reader.GetName(i));
            string Col(string name, int max)
            {
                if (!columns.Contains(name)) return string.Empty;
                var v = reader[name];
                var s = v == DBNull.Value ? string.Empty : (v.ToString() ?? string.Empty).Trim();
                return s.Length > max ? s[..max] : s;
            }

            var tel = Col("Phone", 60);
            if (tel.Length == 0) tel = Col("TelOffice", 60);
            return new NisCompanyInfoDto
            {
                NameTh = Col("CmpName", 200),
                NameEn = Col("CmpNameEN", 200),
                AddressTh = Col("CmpAddress", 400),
                AddressEn = Col("CmpAddressEN", 400),
                Tel = tel,
                Email = Col("Email", 160),
                Website = Col("WebSite", 160),
                TaxId = Col("CmpTaxid", 40),
                LineId = Col("LineId", 60),
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "NSR company: read master company info failed (cmpId={CmpId}) — fallback to InfoJson", cmpId);
            return null;
        }
    }

    /// <summary>serialize เป็น JSON camelCase (รูป CompanyInfoSchema) — ใช้ทั้งเก็บลงฐานและแช่แข็งลง snapshot</summary>
    public static string Serialize(NisCompanyInfoDto dto) => JsonConvert.SerializeObject(dto, CamelCase);

    /// <summary>JObject (รูป CompanyInfoSchema) → DTO · ใช้อ่าน context.company ที่แช่แข็งใน snapshot ด้วย</summary>
    public static NisCompanyInfoDto FromJson(JObject? o) => Parse(o, out _);

    /// <summary>DTO → JObject camelCase</summary>
    public static JObject ToJson(NisCompanyInfoDto dto) => JObject.Parse(Serialize(dto));

    private static NisCompanyInfoResponseDto WithGaps(NisCompanyInfoDto c, bool masterAvailable) => new()
    {
        NameTh = c.NameTh, NameEn = c.NameEn, AddressTh = c.AddressTh, AddressEn = c.AddressEn,
        Tel = c.Tel, Email = c.Email, Website = c.Website, TaxId = c.TaxId, LineId = c.LineId, LineQrPath = c.LineQrPath,
        SalesContacts = c.SalesContacts,
        GapsBeforeProduction = NisReportSchema.CompanyGapsBeforeProduction(c),
        MasterFields = MasterFields.ToList(),
        MasterAvailable = masterAvailable,
    };

    /// <summary>
    /// CompanyInfoSchema.parse — ทุกช่องมี default '' · ตรวจความยาว · lineQrPath ต้องเป็นไฟล์ที่นำเข้าผ่านระบบ
    /// (null → DTO ว่าง ไม่มี issue)
    /// </summary>
    private static NisCompanyInfoDto Parse(JObject? o, out List<string> issues)
    {
        issues = new List<string>();
        var dto = new NisCompanyInfoDto();
        if (o == null) return dto;

        dto.NameTh = Str(o, "nameTh", 200, issues);
        dto.NameEn = Str(o, "nameEn", 200, issues);
        dto.AddressTh = Str(o, "addressTh", 400, issues);
        dto.AddressEn = Str(o, "addressEn", 400, issues);
        dto.Tel = Str(o, "tel", 60, issues);
        dto.Email = Str(o, "email", 160, issues);
        dto.Website = Str(o, "website", 160, issues);
        dto.TaxId = Str(o, "taxId", 40, issues);
        dto.LineId = Str(o, "lineId", 60, issues);
        dto.LineQrPath = AssetPath(o, "lineQrPath", issues);

        var contacts = o["salesContacts"];
        if (contacts is JArray arr)
        {
            if (arr.Count > 30) issues.Add("salesContacts: มีได้ไม่เกิน 30 คน");
            for (var i = 0; i < arr.Count; i++)
            {
                if (arr[i] is not JObject c) { issues.Add($"salesContacts.{i}: ต้องเป็น object"); continue; }
                var p = $"salesContacts.{i}";
                var id = c["id"] is { Type: JTokenType.String } idt ? (string)idt! : null;
                if (string.IsNullOrEmpty(id)) issues.Add($"{p}.id: ต้องระบุ");
                else if (id.Length > 40) issues.Add($"{p}.id: ยาวเกิน 40 ตัวอักษร");
                var ids = new List<string>();
                if (c["customerIds"] is JArray cids)
                {
                    if (cids.Count > 500) issues.Add($"{p}.customerIds: มีได้ไม่เกิน 500 รายการ");
                    foreach (var x in cids)
                    {
                        var s = (string?)x ?? "";
                        if (s.Length == 0 || s.Length > 64) { issues.Add($"{p}.customerIds: รหัสลูกค้าต้องยาว 1–64 ตัวอักษร"); continue; }
                        ids.Add(s);
                    }
                }
                else if (c["customerIds"] != null && c["customerIds"]!.Type != JTokenType.Null)
                {
                    issues.Add($"{p}.customerIds: ต้องเป็น array");
                }
                dto.SalesContacts.Add(new NisSalesContactDto
                {
                    Id = id ?? string.Empty,
                    Name = Str(c, "name", 120, issues, p),
                    Title = Str(c, "title", 120, issues, p),
                    Tel = Str(c, "tel", 60, issues, p),
                    Email = Str(c, "email", 160, issues, p),
                    LineQrPath = AssetPath(c, "lineQrPath", issues, p),
                    CustomerIds = ids,
                });
            }
        }
        else if (contacts != null && contacts.Type != JTokenType.Null)
        {
            issues.Add("salesContacts: ต้องเป็น array");
        }
        return dto;
    }

    private static string Str(JObject o, string key, int max, List<string> issues, string? prefix = null)
    {
        var path = prefix == null ? key : $"{prefix}.{key}";
        var v = o[key];
        if (v == null || v.Type == JTokenType.Null) return string.Empty;
        if (v.Type != JTokenType.String) { issues.Add($"{path}: ต้องเป็นข้อความ"); return string.Empty; }
        var s = (string)v!;
        if (s.Length > max) issues.Add($"{path}: ยาวเกิน {max} ตัวอักษร");
        return s;
    }

    private static string AssetPath(JObject o, string key, List<string> issues, string? prefix = null)
    {
        var s = Str(o, key, 400, issues, prefix);
        if (s.Length > 0 && !NisReportSchema.AssetPathRegex.IsMatch(s))
            issues.Add($"{(prefix == null ? key : $"{prefix}.{key}")}: path QR ต้องเป็นไฟล์ที่นำเข้าผ่านระบบ (/api/nis/report/assets/…)");
        return s;
    }
}
