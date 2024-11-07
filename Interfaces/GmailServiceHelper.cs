using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class GmailServiceHelper
{
    private static string[] Scopes = { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailSend };
    private static string ApplicationName = "webapigmail";
    private GmailService _service;
    private readonly IWebHostEnvironment _env;
    public GmailServiceHelper(IWebHostEnvironment env)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
        InitializeGmailService().Wait();
    }

    public GmailServiceHelper()
    {
    }

    public async Task<IList<Label>> GetLabelsAsync(string userId = "me")
    {
        var request = _service.Users.Labels.List(userId);
        var response = await request.ExecuteAsync();
        return response.Labels;
    }


    private async Task InitializeGmailService()
    {
        UserCredential credential;
        string fileConfig = Path.Combine(_env.ContentRootPath, "config", "client_secret.json");


        using (var stream = new FileStream(fileConfig, FileMode.Open, FileAccess.Read))
        {
            var credPath = "token.json";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }

        _service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    // GET: Fetch emails
    public async Task<IList<Message>> GetEmailsAsync(string userId = "me")
    {
        var request = _service.Users.Messages.List(userId);
        request.LabelIds = "INBOX";  // Fetches only inbox messages
        var response = await request.ExecuteAsync();

        return response.Messages;
    }

    // SET: Send email
    public async Task SendEmailAsync(string userId, string recipient, string subject, string body)
    {
        var msg = new Message
        {
            Raw = EncodeMessageToBase64(new MimeKit.MimeMessage
            {
                From = { new MimeKit.MailboxAddress("Amnart Kongpet", "brambroza@gmail.com") },
                To = { new MimeKit.MailboxAddress(recipient, recipient) },
                Subject = subject,
                Body = new MimeKit.TextPart("plain") { Text = body }
            }.ToString())
        };
        await _service.Users.Messages.Send(msg, userId).ExecuteAsync();
    }

    private static string EncodeMessageToBase64(string message)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message))
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}
