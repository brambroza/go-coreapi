
namespace goalongapi.Dtos;

public class ServiceModeDto
{
    public string CmpId { get; set; } = string.Empty;
    public string ServiceModeId { get; set; } = string.Empty;
    public string? Descriptions { get; set; }
    public int? StateActive { get; set; }
    public string? UpdUser { get; set; }
}