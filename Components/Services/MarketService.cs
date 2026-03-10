using System.Net.Http.Json;
using System.Text.Json;
using InvestEasy.Models;
using Microsoft.Extensions.Caching.Memory;

namespace InvestEasy.Services;


public class MarketService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;

    // Injects HttpClientFactory and config for API key and base URL.
    public MarketService(IHttpClientFactory httpFactory, IConfiguration config, IMemoryCache cache)
    {
        _httpFactory = httpFactory;
        _config = config;
        _cache = cache;
    }

    // Calls Finnhub "quote endpoint". deserialize JSON into IndexQuote-model.
    public async Task<IndexQuote?> GetIndexQuoteAsync(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
        {
            return null;
        }

        symbol = symbol.Trim().ToUpperInvariant();
        string cacheKey = $"quote:{symbol}";

        // checks if the quote exists in the in-memory cache.
        // if exists, it returns the cached value instead of calling API.
        if (_cache.TryGetValue(cacheKey, out IndexQuote? cachedQuote))
        {
            Console.WriteLine($"Using cached quote for {symbol}");
            return cachedQuote;
        }

        Console.WriteLine($"Calling Finnhub API for {symbol}");
        try
        {
            var client = _httpFactory.CreateClient("FinnhubClient");
            var apiKey = _config["Finnhub:ApiKey"];


            var response = await client.GetFromJsonAsync<IndexQuote>(
                $"quote?symbol={symbol}&token={apiKey}");

            // if response is good, store it in cache for 60sec to reduce API calls.
            if (response is not null)
            {
                _cache.Set(cacheKey, response, TimeSpan.FromSeconds(60));
            }

            return response;
        }
        catch
        {
            return null;
        }
    }

    // GET´s market news articles.
    public async Task<List<NewsArticle>> GetMarketNewsAsync(string category = "general")
    {
        var client = _httpFactory.CreateClient("FinnhubClient");
        var apiKey = _config["Finnhub:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Exception("ApiKey is missing. Check appsettings.json!");
        }

        // category = general. 
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

    // search for symbol with stock name.
    public async Task<List<SymbolSearchItem>> SearchSymbolAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return new();
        }

        var client = _httpFactory.CreateClient("FinnhubClient");
        var apiKey = _config["Finnhub:ApiKey"];

        var url = $"search?q={Uri.EscapeDataString(query.Trim())}&token={apiKey}";
        var res = await client.GetFromJsonAsync<SymbolSearchResponse>(url);

        return res?.Result ?? new();
    }
}