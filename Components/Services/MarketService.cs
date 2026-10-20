using System.Net.Http.Json;
using InvestEasy.Models;
using Microsoft.Extensions.Caching.Memory;

namespace InvestEasy.Services;

// Calls the Finnhub API. The free plan allows 60 calls per minute for the whole app,
// so every response is cached in IMemoryCache, which is shared by all users.
public class MarketService
{
    private static readonly TimeSpan QuoteCacheTime = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan NewsCacheTime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SearchCacheTime = TimeSpan.FromMinutes(30);

    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MarketService> _logger;

    // Injects HttpClientFactory and config for API key and base URL.
    public MarketService(IHttpClientFactory httpFactory, IConfiguration config, IMemoryCache cache, ILogger<MarketService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    // Calls Finnhub "quote endpoint". deserialize JSON into IndexQuote-model.
    // Returns null if the quote couldn't be fetched.
    public async Task<IndexQuote?> GetIndexQuoteAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        symbol = symbol.Trim().ToUpperInvariant();

        return await GetCachedAsync<IndexQuote>(
            $"quote:{symbol}",
            $"quote?symbol={Uri.EscapeDataString(symbol)}",
            QuoteCacheTime);
    }

    // GET´s market news articles. Throws if the news couldn't be fetched, so the page can show an error.
    public async Task<List<NewsArticle>> GetMarketNewsAsync(string category = "general")
    {
        var items = await GetCachedAsync<List<NewsArticle>>(
            $"news:{category}",
            $"news?category={Uri.EscapeDataString(category)}",
            NewsCacheTime);

        return items ?? throw new InvalidOperationException("Market news is unavailable.");
    }

    // search for symbol with stock name. Throws if the search failed.
    public async Task<List<SymbolSearchItem>> SearchSymbolAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new();
        }

        query = query.Trim().ToLowerInvariant();

        var res = await GetCachedAsync<SymbolSearchResponse>(
            $"search:{query}",
            $"search?q={Uri.EscapeDataString(query)}",
            SearchCacheTime);

        return res?.Result ?? throw new InvalidOperationException("Symbol search is unavailable.");
    }

    // Returns the cached value if it exists, otherwise calls Finnhub and caches a successful response.
    // Returns null if the API key is missing or the call fails, ex. when the rate limit is reached.
    private async Task<T?> GetCachedAsync<T>(string cacheKey, string pathAndQuery, TimeSpan cacheTime) where T : class
    {
        if (_cache.TryGetValue(cacheKey, out T? cached))
        {
            return cached;
        }

        var apiKey = _config["Finnhub:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        try
        {
            var client = _httpFactory.CreateClient("FinnhubClient");
            using var request = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);
            request.Headers.Add("X-Finnhub-Token", apiKey);

            using var httpResponse = await client.SendAsync(request);
            httpResponse.EnsureSuccessStatusCode();
            var response = await httpResponse.Content.ReadFromJsonAsync<T>();

            if (response is not null)
            {
                _cache.Set(cacheKey, response, cacheTime);
            }

            return response;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Finnhub request {CacheKey} failed", cacheKey);
            return null;
        }
    }
}
