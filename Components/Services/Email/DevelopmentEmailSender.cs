using InvestEasy.Data;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace InvestEasy.Services.Email;

// Used in development when no SMTP server is configured. Writes the links to the log
// instead of sending them, so the account flows can be tested locally.
public class DevelopmentEmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        _logger.LogWarning("Email not sent (no SMTP configured). Confirmation link for {Email}: {Link}", email, WebUtility.HtmlDecode(confirmationLink));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        _logger.LogWarning("Email not sent (no SMTP configured). Password reset link for {Email}: {Link}", email, WebUtility.HtmlDecode(resetLink));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        _logger.LogWarning("Email not sent (no SMTP configured). Password reset code for {Email}: {Code}", email, resetCode);
        return Task.CompletedTask;
    }
}
