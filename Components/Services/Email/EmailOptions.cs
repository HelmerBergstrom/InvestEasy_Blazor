namespace InvestEasy.Services.Email;

// SMTP settings from the "Email" section in configuration. The password is a secret and is set
// with user-secrets locally and as the environment variable Email__Password in production.
public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "InvestEasy";

    // Only turn off for a local test mail server, ex. Mailpit. Real providers always use TLS.
    public bool UseTls { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(FromAddress);
}
