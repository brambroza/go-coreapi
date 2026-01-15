namespace goalongapi.Dtos;

public record ReportTemplateListDto(
  Guid TemplateId,
  string TemplateCode,
  string TemplateName,
  int Version,
  bool IsActive,
  DateTime UpdatedAt
);

public record ReportTemplateDetailDto(
  Guid TemplateId,
  string TemplateCode,
  string TemplateName,
  int Version,
  bool IsActive,
  string ConfigJson,
  DateTime UpdatedAt
);

public record CreateTemplateRequest(
  string TemplateCode,
  string TemplateName,
  string ConfigJson,
  string? User
);

public record UpdateTemplateRequest(
  string TemplateName,
  string ConfigJson,
  string? User,
  bool CreateNewVersion = true
);
