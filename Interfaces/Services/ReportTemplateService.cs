using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using goalongapi.Data;
using goalongapi.Entities;
using goalongapi.Interfaces; 
using goalongapi.Dtos;

namespace goalongapi.Services;

public class ReportTemplateService : IReportTemplateService
{ 
    private readonly DatabaseContext _db;
    public ReportTemplateService(DatabaseContext db) => _db = db;

    private static void ValidateJson(string json)
    {
        try { JsonDocument.Parse(json); }
        catch (Exception ex) { throw new ArgumentException("ConfigJson is not valid JSON: " + ex.Message); }
    }

    public async Task<List<ReportTemplateListDto>> ListAsync()
    {
        return await _db.ReportTemplates
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new ReportTemplateListDto(
                x.TemplateId, x.TemplateCode, x.TemplateName, x.Version, x.IsActive, x.UpdatedAt
            ))
            .ToListAsync();
    }

    public async Task<ReportTemplateDetailDto?> GetAsync(Guid id)
    {
        var x = await _db.ReportTemplates.FirstOrDefaultAsync(t => t.TemplateId == id);
        if (x is null) return null;

        return new ReportTemplateDetailDto(
            x.TemplateId, x.TemplateCode, x.TemplateName, x.Version, x.IsActive, x.ConfigJson, x.UpdatedAt
        );
    }

    public async Task<ReportTemplateDetailDto> CreateAsync(CreateTemplateRequest req)
    {
        ValidateJson(req.ConfigJson);

        // version = max+1 per code (หรือเริ่ม 1)
        var maxVer = await _db.ReportTemplates
            .Where(x => x.TemplateCode == req.TemplateCode)
            .MaxAsync(x => (int?)x.Version);

        var ver = (maxVer ?? 0) + 1;

        // ทำให้ active เป็นของ version ใหม่ (ถ้าต้องการ)
        var actives = await _db.ReportTemplates
            .Where(x => x.TemplateCode == req.TemplateCode && x.IsActive)
            .ToListAsync();
        foreach (var a in actives) a.IsActive = false;

        var ent = new ReportTemplate
        {
            TemplateCode = req.TemplateCode,
            TemplateName = req.TemplateName,
            Version = ver,
            ConfigJson = req.ConfigJson,
            IsActive = true,
            CreatedBy = req.User,
            UpdatedBy = req.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ReportTemplates.Add(ent);
        await _db.SaveChangesAsync();

        return new ReportTemplateDetailDto(ent.TemplateId, ent.TemplateCode, ent.TemplateName, ent.Version, ent.IsActive, ent.ConfigJson, ent.UpdatedAt);
    }

    public async Task<ReportTemplateDetailDto?> UpdateAsync(Guid id, UpdateTemplateRequest req)
    {
        ValidateJson(req.ConfigJson);

        var current = await _db.ReportTemplates.FirstOrDefaultAsync(t => t.TemplateId == id);
        if (current is null) return null;

        if (!req.CreateNewVersion)
        {
            current.TemplateName = req.TemplateName;
            current.ConfigJson = req.ConfigJson;
            current.UpdatedBy = req.User;
            current.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new ReportTemplateDetailDto(current.TemplateId, current.TemplateCode, current.TemplateName, current.Version, current.IsActive, current.ConfigJson, current.UpdatedAt);
        }

        // new version
        var maxVer = await _db.ReportTemplates
            .Where(x => x.TemplateCode == current.TemplateCode)
            .MaxAsync(x => (int?)x.Version);

        var ver = (maxVer ?? 0) + 1;

        // deactivate old actives
        var actives = await _db.ReportTemplates
            .Where(x => x.TemplateCode == current.TemplateCode && x.IsActive)
            .ToListAsync();
        foreach (var a in actives) a.IsActive = false;

        var ent = new ReportTemplate
        {
            TemplateCode = current.TemplateCode,
            TemplateName = req.TemplateName,
            Version = ver,
            ConfigJson = req.ConfigJson,
            IsActive = true,
            CreatedBy = current.CreatedBy,
            UpdatedBy = req.User,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ReportTemplates.Add(ent);
        await _db.SaveChangesAsync();

        return new ReportTemplateDetailDto(ent.TemplateId, ent.TemplateCode, ent.TemplateName, ent.Version, ent.IsActive, ent.ConfigJson, ent.UpdatedAt);
    }

    public async Task<bool> SetActiveAsync(string templateCode, int version)
    {
        var all = await _db.ReportTemplates.Where(x => x.TemplateCode == templateCode).ToListAsync();
        if (all.Count == 0) return false;

        foreach (var t in all) t.IsActive = (t.Version == version);
        foreach (var t in all) t.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }
}
