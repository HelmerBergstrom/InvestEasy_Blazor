using System.Net;

namespace InvestEasy.Services.Email;

// Builds the HTML and plain text versions of the account emails.
public static class EmailTemplate
{
    // The link is already HTML encoded by the Identity pages.
    public static (string Html, string Text) Build(string heading, string message, string buttonText, string encodedLink)
    {
        var link = WebUtility.HtmlDecode(encodedLink);

        var html = $"""
            <!DOCTYPE html>
            <html lang="en">
            <body style="margin:0;padding:24px;background:#f1f5f9;font-family:system-ui,-apple-system,'Segoe UI',Roboto,Arial,sans-serif;color:#0f172a;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:520px;margin:0 auto;background:#ffffff;border-radius:12px;">
                    <tr>
                        <td style="padding:32px;">
                            <p style="margin:0 0 24px;font-size:20px;font-weight:700;">Invest<span style="color:#16a34a;">Easy</span></p>
                            <h1 style="margin:0 0 12px;font-size:22px;">{WebUtility.HtmlEncode(heading)}</h1>
                            <p style="margin:0 0 24px;font-size:15px;line-height:1.6;color:#334155;">{WebUtility.HtmlEncode(message)}</p>
                            <a href="{encodedLink}" style="display:inline-block;padding:12px 20px;background:#16a34a;color:#ffffff;text-decoration:none;font-weight:600;border-radius:8px;">{WebUtility.HtmlEncode(buttonText)}</a>
                            <p style="margin:24px 0 0;font-size:13px;line-height:1.6;color:#64748b;">
                                If the button doesn't work, copy this link into your browser:<br />
                                <a href="{encodedLink}" style="color:#16a34a;word-break:break-all;">{encodedLink}</a>
                            </p>
                            <p style="margin:24px 0 0;font-size:13px;color:#64748b;">If you didn't ask for this email, you can ignore it.</p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        var text = $"""
            {heading}

            {message}

            {buttonText}: {link}

            If you didn't ask for this email, you can ignore it.
            """;

        return (html, text);
    }
}
