namespace InvestEasy.Models;

public class WatchListItemModel
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    // Finnhub symbol for search of quote
    public string Symbol { get; set; } = "";

    // Name of the stock
    public string Name { get; set; } = "";

    // ex: Common stock, ETF etc.
    public string? Type { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}