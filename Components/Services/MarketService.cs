using System.Net.Http.Json;
using System.Text.Json;
using InvestEasy.Models;

namespace InvestEasy.Services;


public class MarketService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;

    // Injects HttpClientFactory and config for API key and base URL.
    public MarketService(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config = config;
    }

    // Calls Finnhub "quote endpoint". deserialize JSON into IndexQuote-model.
    public async Task<IndexQuote?> GetIndexQuoteAsync(string symbol)
    {
        var client = _httpFactory.CreateClient("FinnhubClient");
        var apiKey = _config["Finnhub:ApiKey"];

        var response = await client.GetFromJsonAsync<IndexQuote>(
            $"quote?symbol={symbol}&token={apiKey}");

        return response;
    }
    public async Task<List<NewsArticle>> GetMarketNewsAsync(string category = "general")
    {
        var client = _httpFactory.CreateClient("FinnhubClient");
        var apiKey = _config["Finnhub:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("ApiKey is missing. Check appsettings.json!");
        }

        var url = $"news?category={category}&token={apiKey}";

        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        // Deserialize from json-text to C#-object.
        var items = JsonSerializer.Deserialize<List<NewsArticle>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return items ?? new List<NewsArticle>();
    }

    public async Task<List<SymbolSearchItem>> SearchSymbolAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return new();
        }

        var client = _httpFactory.CreateClient("FinnhubClient");
        var apiKey = _config["Finnhub:ApiKey"];

        var url = $"search?={Uri.EscapeDataString(query.Trim())}&token={apiKey}";
        var res = await client.GetFromJsonAsync<SymbolSearchResponse>(url);

        return res?.Result ?? new();
    }
}