using goalongapi.Dtos;

namespace goalongapi.Services;

public interface IReportTemplateService
{
    Task<List<ReportTemplateListDto>> ListAsync();
    Task<ReportTemplateDetailDto?> GetAsync(Guid id);
    Task<ReportTemplateDetailDto> CreateAsync(CreateTemplateRequest req);
    Task<ReportTemplateDetailDto?> UpdateAsync(Guid id, UpdateTemplateRequest req);
    Task<bool> SetActiveAsync(string templateCode, int version);
}
