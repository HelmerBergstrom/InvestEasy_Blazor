using InvestEasy.Services.Email;

namespace InvestEasy.Tests;

public class EmailTemplateTests
{
    [Fact]
    public void Link_IsKeptEncodedInHtml_AndDecodedInText()
    {
        var (html, text) = EmailTemplate.Build("Heading", "Message", "Click", "https://example.com/reset?a=1&amp;code=x");

        Assert.Contains("href=\"https://example.com/reset?a=1&amp;code=x\"", html);
        Assert.Contains("https://example.com/reset?a=1&code=x", text);
    }

    [Fact]
    public void Text_IsHtmlEncoded()
    {
        var (html, _) = EmailTemplate.Build("<script>", "a & b", "Go", "https://example.com");

        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("a &amp; b", html);
        Assert.DoesNotContain("<script>", html);
    }
}
