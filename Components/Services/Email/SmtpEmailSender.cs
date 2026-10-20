using InvestEasy.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;

namespace InvestEasy.Services.Email;

// Sends the Identity emails (confirm account, reset password) through SMTP.
public class SmtpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var (html, text) = EmailTemplate.Build(
            "Confirm your email",
            "Welcome to InvestEasy! Confirm your email address to activate your account.",
            "Confirm email",
            confirmationLink);

        return SendAsync(email, "Confirm your InvestEasy account", html, text);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var (html, text) = EmailTemplate.Build(
            "Reset your password",
            "We received a request to reset the password for your InvestEasy account. The link is valid for one day.",
            "Reset password",
            resetLink);

        return SendAsync(email, "Reset your InvestEasy password", html, text);
    }

    // Not used by the pages in this app, which send reset links instead of codes.
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Reset your InvestEasy password",
            $"<p>Your password reset code is: <strong>{System.Net.WebUtility.HtmlEncode(resetCode)}</strong></p>",
            $"Your password reset code is: {resetCode}");

    // Failures are logged but not thrown. An error page would tell the visitor whether the email
    // address has an account, and the user can ask for a new email.
    private async Task SendAsync(string to, string subject, string html, string text)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = html, TextBody = text }.ToMessageBody();

        try
        {
            using var client = new SmtpClient { Timeout = 15000 };

            // Port 465 uses TLS from the start, other ports (587) upgrade with STARTTLS.
            var security = !_options.UseTls ? SecureSocketOptions.None
                : _options.Port == 465 ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTls;
            await client.ConnectAsync(_options.Host, _options.Port, security);

            if (!string.IsNullOrEmpty(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation("Sent email \"{Subject}\"", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't send email \"{Subject}\"", subject);
        }
    }
}
