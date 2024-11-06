using Google.Apis.Gmail.v1.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly GmailServiceHelper _gmailService;

    public MailController()
    {
        _gmailService = new GmailServiceHelper();
    }

    [HttpGet("emails")]
    public async Task<ActionResult<IEnumerable<Message>>> GetEmails()
    {
        var emails = await _gmailService.GetEmailsAsync();
        return Ok(emails);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest emailRequest)
    {
        await _gmailService.SendEmailAsync("me", emailRequest.To, emailRequest.Subject, emailRequest.Body);
        return Ok("Email sent successfully!");
    }

    [HttpGet("labels")]
    public async Task<ActionResult<IEnumerable<Label>>> GetLabels()
    {
        var labels = await _gmailService.GetLabelsAsync();
        return Ok(labels);
    }

}

public class EmailRequest
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
