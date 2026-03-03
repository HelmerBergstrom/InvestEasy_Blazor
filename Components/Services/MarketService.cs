using System.Net.Http.Json;
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
}