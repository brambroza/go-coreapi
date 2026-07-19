using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using MimeKit;
using goalongapi.Models;

namespace goalongapi.Helpers;

/// A file to attach to an outgoing email (already-decoded bytes, not base64).
public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType = "application/octet-stream");

public sealed class GoogleOAuthMailService
{
    private const string GoogleScopes = "https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/calendar.events";
    private readonly EmailSettingRepository _repo;
    private readonly AesCrypto _crypto;
    private readonly IConfiguration _configuration;
    private readonly IDataProtector _stateProtector;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleOAuthMailService(EmailSettingRepository repo, AesCrypto crypto, IConfiguration configuration, IDataProtectionProvider dataProtectionProvider, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _crypto = crypto;
        _configuration = configuration;
        _stateProtector = dataProtectionProvider.CreateProtector("goalong.google-oauth-mail.state.v1");
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CreateAuthorizationUrlAsync(string? cmpId, string settingName)
    {
        var setting = await _repo.GetActiveAsync(cmpId, settingName)
            ?? throw new InvalidOperationException("Email SMTP setting not found.");
        if (string.IsNullOrWhiteSpace(setting.GoogleOAuthClientId) || setting.GoogleOAuthClientSecretEnc.Length == 0)
            throw new InvalidOperationException("Google OAuth client configuration is not stored for this mail setting.");

        var redirectUri = RequiredRedirectUri();
        var state = _stateProtector.Protect(JsonSerializer.Serialize(new OAuthState(cmpId, settingName)));
        return "https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={Uri.EscapeDataString(setting.GoogleOAuthClientId)}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               "response_type=code&access_type=offline&prompt=consent&" +
               $"scope={Uri.EscapeDataString(GoogleScopes)}&state={Uri.EscapeDataString(state)}";
    }

    public async Task CompleteAuthorizationAsync(string code, string protectedState)
    {
        OAuthState state;
        try { state = JsonSerializer.Deserialize<OAuthState>(_stateProtector.Unprotect(protectedState))!; }
        catch (Exception ex) { throw new InvalidOperationException("Google OAuth state is invalid or expired.", ex); }

        var setting = await _repo.GetActiveAsync(state.CmpId, state.SettingName)
            ?? throw new InvalidOperationException("Email SMTP setting no longer exists.");
        var clientSecret = DecryptClientSecret(setting);

        var http = _httpClientFactory.CreateClient();
        using var response = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = setting.GoogleOAuthClientId!,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = RequiredRedirectUri(),
            ["grant_type"] = "authorization_code",
        }));
        var payload = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Google OAuth token exchange failed: " + payload);

        using var json = JsonDocument.Parse(payload);
        if (!json.RootElement.TryGetProperty("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken.GetString()))
            throw new InvalidOperationException("Google did not return a refresh token. Revoke the app access and authorize again.");

        var (cipher, iv) = _crypto.Encrypt(refreshToken.GetString()!);
        if (!await _repo.UpdateGoogleOAuthRefreshTokenAsync(state.CmpId, state.SettingName, cipher, iv))
            throw new InvalidOperationException("Could not save the Google OAuth refresh token.");
    }

    /// Send an HTML email through the Gmail API (OAuth). When <paramref name="attachments"/> is null/empty
    /// the message is a single text/html part (same result as before, but built with MimeKit so that
    /// non-ASCII subjects/addresses are RFC 2047 encoded correctly); when attachments are supplied the
    /// message becomes multipart/mixed. Keeping the trailing parameter optional preserves the existing
    /// 4-argument signature used by MailController and the onsite-report caller.
    public async Task SendAsync(EmailSmtpSetting setting, string recipientEmail, string subject, string htmlBody, IReadOnlyList<EmailAttachment>? attachments = null)
    {
        if (setting.GoogleOAuthRefreshTokenEnc.Length == 0)
            throw new InvalidOperationException("Google OAuth is not connected for this mail setting.");

        var accessToken = await GetAccessTokenAsync(setting);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(setting.FromName ?? "GoAlong Support", setting.FromEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        // Strip CR/LF to keep the header injection guard the string-built version had.
        message.Subject = (subject ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments != null)
        {
            foreach (var att in attachments)
            {
                if (att.Content.Length == 0) continue;
                builder.Attachments.Add(att.FileName, att.Content, ContentType.Parse(att.ContentType));
            }
        }
        message.Body = builder.ToMessageBody();

        using var ms = new MemoryStream();
        await message.WriteToAsync(ms);
        var raw = Convert.ToBase64String(ms.ToArray()).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await http.PostAsync("https://gmail.googleapis.com/gmail/v1/users/me/messages/send",
            new StringContent(JsonSerializer.Serialize(new { raw }), Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Gmail send failed: " + await response.Content.ReadAsStringAsync());
    }

    public async Task<string> GetAccessTokenAsync(EmailSmtpSetting setting)
    {
        var refreshToken = _crypto.Decrypt(setting.GoogleOAuthRefreshTokenEnc, setting.GoogleOAuthRefreshTokenIv);
        var http = _httpClientFactory.CreateClient();
        using var response = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = setting.GoogleOAuthClientId!,
            ["client_secret"] = DecryptClientSecret(setting),
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token",
        }));
        var payload = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Google OAuth token refresh failed: " + payload);
        using var json = JsonDocument.Parse(payload);
        return json.RootElement.GetProperty("access_token").GetString()!;
    }

    private string RequiredRedirectUri() => _configuration["GoogleOAuth:RedirectUri"]
        ?? throw new InvalidOperationException("GoogleOAuth:RedirectUri is missing.");

    private string DecryptClientSecret(EmailSmtpSetting setting) => _crypto.Decrypt(setting.GoogleOAuthClientSecretEnc, setting.GoogleOAuthClientSecretIv);

    private sealed record OAuthState(string? CmpId, string SettingName);
}
