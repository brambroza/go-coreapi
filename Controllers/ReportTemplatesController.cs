using Microsoft.AspNetCore.Mvc;
using goalongapi.Dtos;
using goalongapi.Services;

namespace goalongapi.Controllers;

[ApiController]
[Route("api/report-templates")]
public class ReportTemplatesController : ControllerBase
{
    private readonly IReportTemplateService _svc;
    public ReportTemplatesController(IReportTemplateService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _svc.ListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var x = await _svc.GetAsync(id);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest req)
    {
        var created = await _svc.CreateAsync(req);
        return CreatedAtAction(nameof(Get), new { id = created.TemplateId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateRequest req)
    {
        var updated = await _svc.UpdateAsync(id, req);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("{templateCode}/set-active/{version:int}")]
    public async Task<IActionResult> SetActive(string templateCode, int version)
    {
        var ok = await _svc.SetActiveAsync(templateCode, version);
        return ok ? Ok(new { templateCode, version }) : NotFound();
    }
}
