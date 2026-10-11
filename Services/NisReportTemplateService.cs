using goalongapi.Data;
using goalongapi.Dtos.Nis;
using goalongapi.Models.Nis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace goalongapi.Services;

/// <summary>
/// Master Template ของรายงานบริการ — port จาก apps/api/src/reports/templates.service.ts
///
/// 🔴 ตรวจ layout ด้วย <see cref="NisReportSchema.Normalize"/> ทุกครั้งที่เขียน และทุกครั้งที่อ่านออก (ไม่เชื่อว่าในฐานถูกเสมอ)
/// 🔴 ไม่มีการลบ template จริง — "ลบ" คือ ArchivedAt (รายงานที่เคยออกยังเปิดดูได้)
/// 🔴 Version +1 เฉพาะเมื่อ layout เปลี่ยนจริง (auto-save ของ builder เรียกทุกไม่กี่วินาที)
/// 🔴 ห้ามสร้าง/แก้ template ที่รหัสรอง (SPEC §9 ข้อ 59) — ช่องระดับเอกสารมาจากรหัสหลักเท่านั้น
/// ลงทะเบียน AddScoped ใน Program.cs (ไม่มี interface)
/// </summary>
public class NisReportTemplateService
{
    private readonly DatabaseContext _db;
    private readonly INisArchiveService _archive;
    private readonly NisCustomerGroupResolverService _groups;

    public NisReportTemplateService(DatabaseContext db, INisArchiveService archive, NisCustomerGroupResolverService groups)
    {
        _db = db;
        _archive = archive;
        _groups = groups;
    }

    /// <summary>รายการ template ของบริษัท — default ก่อน แล้ว updatedAt ใหม่สุดก่อน · ไม่รวมที่เก็บเข้ากรุเว้นแต่ขอ</summary>
    public async Task<List<NisReportTemplateDto>> ListAsync(string cmpId, string? customerId, bool includeArchived, CancellationToken ct = default)
    {
        var q = _db.NisReportTemplates.AsNoTracking().Where(t => t.CmpId == cmpId);
        if (!string.IsNullOrWhiteSpace(customerId)) q = q.Where(t => t.CustomerId == customerId);
        if (!includeArchived) q = q.Where(t => t.ArchivedAt == null);
        var rows = await q.OrderByDescending(t => t.IsDefault).ThenByDescending(t => t.UpdatedAt).ToListAsync(ct);
        return rows.Select(ToDto).ToList();
    }

    /// <summary>template หนึ่งฉบับ — 404 ถ้าไม่พบในบริษัทนี้</summary>
    public async Task<NisReportTemplateDto> ByIdAsync(string cmpId, Guid id, CancellationToken ct = default)
    {
        var row = await _db.NisReportTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.CmpId == cmpId && t.Id == id, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบ template \"{id}\"");
        return ToDto(row);
    }

    /// <summary>entity ดิบ (ให้ NisReportService ใช้ตอนออกรายงาน) — 404 ถ้าไม่พบ</summary>
    public async Task<NisReportTemplate> EntityAsync(string cmpId, Guid id, CancellationToken ct = default) =>
        await _db.NisReportTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.CmpId == cmpId && t.Id == id, ct)
        ?? throw new NisArchiveNotFoundException($"ไม่พบ template \"{id}\"");

    /// <summary>
    /// สร้าง template — layout ต้องผ่าน schema · docMeta.customerId ต้องเท่า customerId · รหัสรอง → 400 · ชื่อซ้ำ → 409
    /// </summary>
    public async Task<NisReportTemplateDto> CreateAsync(string cmpId, string customerId, string name, JToken? layoutRaw, bool? isDefault, string createdBy, CancellationToken ct = default)
    {
        var layout = ParseLayout(layoutRaw);
        var inLayout = NisReportSchema.DocCustomerId(layout);
        if (inLayout != customerId)
            throw new NisReportBadRequestException("รหัสลูกค้าใน layout ไม่ตรงกับที่สร้าง template ให้",
                new Dictionary<string, object?> { ["inLayout"] = inLayout, ["requested"] = customerId });

        var group = await _groups.ResolveGroupAsync(cmpId, customerId, ct);
        if (group.IsSecondary)
            throw new NisReportBadRequestException(
                $"\"{customerId}\" เป็นรหัสรองที่ถูกรวมเข้ากับรหัส \"{group.PrimaryCustomerId}\" แล้ว — " +
                $"สร้าง/แก้ template ที่รหัสหลัก \"{group.PrimaryCustomerId}\" แทน ตัวเลขและรายการเคสจะรวม" +
                "ทุกรหัสในกลุ่มให้เองตอนออกรายงาน");

        var now = DateTimeOffset.UtcNow;
        var row = new NisReportTemplate
        {
            CmpId = cmpId,
            CustomerId = customerId,
            Name = name,
            DocKind = (string?)layout["docKind"] ?? "book",
            LayoutJson = NisReportJson.Compact(layout),
            Version = 1,
            IsDefault = isDefault ?? false,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };
        _db.NisReportTemplates.Add(row);
        await SaveOrConflictAsync(customerId, name, ct);
        if (row.IsDefault) await ClearOtherDefaultsAsync(cmpId, customerId, row.Id, ct);
        return ToDto(row);
    }

    /// <summary>
    /// สร้างจากโครงสำเร็จรูป — caseCount = จำนวนเคสแจ้งเข้าจริงของทั้งกลุ่มในช่วง (SPEC §9 ข้อ 24 · 59) ผ่าน
    /// <see cref="INisArchiveService.BuildWhere"/> ตัวเดียวกับหน้าจอ ไม่ query เองซ้ำ
    /// </summary>
    public async Task<NisReportTemplateDto> CreateFromStarterAsync(string cmpId, NisCreateStarterDto input, string createdBy, CancellationToken ct = default)
    {
        var group = await _groups.ResolveGroupAsync(cmpId, input.CustomerId, ct);
        var filter = new NisCaseFilter
        {
            From = input.From,
            To = input.To,
            DateField = "createdAt",
            CustomerId = group.MemberCustomerIds,
        };
        var issues = filter.Validate();
        if (issues.Count > 0)
            throw new NisReportBadRequestException("ข้อมูลที่ส่งมาไม่ถูกต้อง", new Dictionary<string, object?> { ["issues"] = issues });
        var caseCount = await _archive.BuildWhere(cmpId, filter).CountAsync(ct);

        var layout = NisReportSchema.StarterTemplate(input.DocKind, new NisStarterOptions
        {
            CustomerId = input.CustomerId,
            CustomerNameTh = input.CustomerName,
            From = input.From,
            To = input.To,
            Preset = input.Preset,
            CaseCount = caseCount,
        });
        return await CreateAsync(cmpId, input.CustomerId, input.Name, layout, true, createdBy, ct);
    }

    /// <summary>
    /// บันทึกทับ — version ขยับเฉพาะเมื่อ layout เปลี่ยนจริง (เทียบ JSON ที่ normalize แล้ว) · เก็บเข้ากรุอยู่ → 409
    /// รหัสลูกค้าใน layout ต้องไม่สลับ · ตั้ง default จะปลด default ตัวอื่นของลูกค้ารายเดียวกัน
    /// </summary>
    public async Task<NisReportTemplateDto> UpdateAsync(string cmpId, Guid id, NisUpdateTemplateDto input, string updatedBy, CancellationToken ct = default)
    {
        var current = await _db.NisReportTemplates.FirstOrDefaultAsync(t => t.CmpId == cmpId && t.Id == id, ct)
                      ?? throw new NisArchiveNotFoundException($"ไม่พบ template \"{id}\"");
        if (current.ArchivedAt != null)
            throw new NisConflictException($"template \"{current.Name}\" ถูกเก็บเข้ากรุแล้ว — เอากลับมาก่อนจึงแก้ได้");

        var changed = false;
        if (input.Name != null && input.Name != current.Name) { current.Name = input.Name; changed = true; }
        if (input.IsDefault != null && input.IsDefault.Value != current.IsDefault) { current.IsDefault = input.IsDefault.Value; changed = true; }

        if (input.Layout is { } layoutEl && layoutEl.ValueKind != System.Text.Json.JsonValueKind.Undefined)
        {
            var layout = ParseLayout(NisReportJson.ToToken(layoutEl));
            var inLayout = NisReportSchema.DocCustomerId(layout);
            if (inLayout != current.CustomerId)
                throw new NisReportBadRequestException("รหัสลูกค้าใน layout ไม่ตรงกับ template เดิม — ห้ามสลับลูกค้าด้วยการแก้ layout",
                    new Dictionary<string, object?> { ["inLayout"] = inLayout, ["template"] = current.CustomerId });

            var nextJson = NisReportJson.Compact(layout);
            var currentNormalized = NisReportJson.TryParseObject(current.LayoutJson) is { } cur
                ? NisReportJson.Compact(NisReportSchema.Normalize(cur, out _))
                : current.LayoutJson;
            if (nextJson != currentNormalized)
            {
                // เช็คซ้ำทุกครั้งที่แก้จริง — ลูกค้าอาจถูกรวมเป็นรหัสรองไปแล้วระหว่างที่ template ยังอยู่
                var group = await _groups.ResolveGroupAsync(cmpId, current.CustomerId, ct);
                if (group.IsSecondary)
                    throw new NisReportBadRequestException(
                        $"\"{current.CustomerId}\" เป็นรหัสรองที่ถูกรวมเข้ากับรหัส \"{group.PrimaryCustomerId}\" แล้ว — " +
                        $"แก้ template ที่รหัสหลัก \"{group.PrimaryCustomerId}\" แทน");
                current.LayoutJson = nextJson;
                current.DocKind = (string?)layout["docKind"] ?? current.DocKind;
                current.Version += 1;
                changed = true;
            }
        }

        if (!changed) return ToDto(current);

        current.UpdatedAt = DateTimeOffset.UtcNow;
        current.UpdatedBy = updatedBy;
        await SaveOrConflictAsync(current.CustomerId, current.Name, ct);
        if (current.IsDefault) await ClearOtherDefaultsAsync(cmpId, current.CustomerId, current.Id, ct);
        return ToDto(current);
    }

    /// <summary>"ลบ" = เก็บเข้ากรุ · เอากลับได้ · เก็บเข้ากรุแล้วต้องไม่เป็นค่าเริ่มต้นค้างไว้</summary>
    public async Task<NisReportTemplateDto> ArchiveAsync(string cmpId, Guid id, bool archived, string updatedBy, CancellationToken ct = default)
    {
        var row = await _db.NisReportTemplates.FirstOrDefaultAsync(t => t.CmpId == cmpId && t.Id == id, ct)
                  ?? throw new NisArchiveNotFoundException($"ไม่พบ template \"{id}\"");
        row.ArchivedAt = archived ? DateTimeOffset.UtcNow : null;
        if (archived) row.IsDefault = false;
        row.UpdatedAt = DateTimeOffset.UtcNow;
        row.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync(ct);
        return ToDto(row);
    }

    /// <summary>layout ของ template เป็น JObject ที่ normalize แล้ว (โยน 400 ถ้าของในฐานอ่านไม่ผ่าน schema ปัจจุบัน)</summary>
    public static JObject LayoutOf(NisReportTemplate row)
    {
        var parsed = NisReportJson.TryParseObject(row.LayoutJson);
        var layout = NisReportSchema.Normalize(parsed, out var issues);
        if (issues.Count > 0)
            throw new NisReportBadRequestException(
                $"template \"{row.Name}\" ที่เก็บไว้อ่านไม่ผ่าน schema ปัจจุบัน — ต้อง migrate ก่อนใช้",
                new Dictionary<string, object?> { ["templateId"] = row.Id, ["issues"] = issues });
        return layout;
    }

    private static NisReportTemplateDto ToDto(NisReportTemplate row)
    {
        var layout = LayoutOf(row);
        var dto = NisReportTemplateDto.From(row);
        dto.Layout = NisReportJson.ToElement(layout);
        return dto;
    }

    /// <summary>TemplateSchema.safeParse — ไม่ผ่าน → 400 พร้อม issues</summary>
    private static JObject ParseLayout(JToken? raw)
    {
        var layout = NisReportSchema.Normalize(raw, out var issues);
        if (issues.Count > 0)
            throw new NisReportBadRequestException("layout ของ template ไม่ถูกต้อง", new Dictionary<string, object?> { ["issues"] = issues });
        return layout;
    }

    private async Task ClearOtherDefaultsAsync(string cmpId, string customerId, Guid keepId, CancellationToken ct)
    {
        await _db.NisReportTemplates
            .Where(t => t.CmpId == cmpId && t.CustomerId == customerId && t.IsDefault && t.Id != keepId)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsDefault, false), ct);
    }

    /// <summary>ชน unique (CmpId, CustomerId, Name) → 409 ด้วยข้อความสำหรับคนที่กดปุ่ม ไม่ใช่ 500</summary>
    private async Task SaveOrConflictAsync(string customerId, string name, CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException { Number: 2601 or 2627 })
        {
            _ = customerId;
            throw new NisConflictException(
                $"ลูกค้ารายนี้มีแบบรายงานชื่อ \"{name}\" อยู่แล้ว — เลือกจากเมนู \"แบบรายงาน\" ด้านบนเพื่อแก้ฉบับเดิม " +
                "หรือเปลี่ยนชื่อก่อนถ้าตั้งใจสร้างฉบับใหม่");
        }
    }
}
