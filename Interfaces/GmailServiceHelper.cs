using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class GmailServiceHelper
{
    private static string[] Scopes = { GmailService.Scope.GmailModify, GmailService.Scope.GmailSend };
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

        try
        {
            var response = await request.ExecuteAsync();
            return response.Labels;
        }
        catch (System.Exception ex)
        {

            var t = ex.Message.ToString();
            Console.WriteLine("tt" + t);
            throw;
        }
    }






    private async Task InitializeGmailService()
    {
        UserCredential credential;
        string fileConfig = Path.Combine(_env.ContentRootPath, "config", "client_secret2.json");


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

    public async Task<IList<Message>> GetEmailsByLabelAsync(string labelId)
    {
        var request = _service.Users.Messages.List("me");
        request.LabelIds = new List<string> { labelId };
        request.MaxResults = 10; // คุณสามารถปรับจำนวนผลลัพธ์ได้ตามต้องการ

        var response = await request.ExecuteAsync();

        if (response.Messages == null || response.Messages.Count == 0)
        {
            return new List<Message>();
        }

        var messages = new List<Message>();
        foreach (var msg in response.Messages)
        {
            var msgRequest = _service.Users.Messages.Get("me", msg.Id);
            messages.Add(await msgRequest.ExecuteAsync());
        }

        return messages;
    }

    public async Task<Message> GetMailByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentNullException(nameof(id), "Mail ID cannot be null or empty.");
        }

        try
        {
            // Assuming you have already authenticated Gmail API service initialized as `_gmailService`
            var request = _service.Users.Messages.Get("me", id); // 'me' refers to the authenticated user
            var message = await request.ExecuteAsync();
            return message;
        }
        catch (Google.GoogleApiException ex)
        {
            if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // Or you can throw an exception based on your application's needs
            }

            throw;
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            throw new Exception($"An error occurred while fetching the mail: {ex.Message}", ex);
        }
    }




    // SET: Send email 
    public async Task SendEmailAsync(string userId, string recipient, string subject, MimeKit.MimeEntity body)
{
    var message = new MimeKit.MimeMessage
    {
        From = { new MimeKit.MailboxAddress("Amnart Kongpet", "brambroza@gmail.com") },
        To = { new MimeKit.MailboxAddress(recipient, recipient) },
        Subject = subject,
        Body = body
    };

    try
    {
         
        var rawMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(message.ToString()))
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        var gmailMessage = new Google.Apis.Gmail.v1.Data.Message
        {
            Raw = rawMessage
        };

        await _service.Users.Messages.Send(gmailMessage, userId).ExecuteAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating or sending the message: {ex.Message}");
        throw;
    }
}





    private static string EncodeMessageToBase64(string message)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message))
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }


}
