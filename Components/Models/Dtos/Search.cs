namespace InvestEasy.Models;

public class SymbolSearchResponse
{
    public int Count { get; set; }
    public List<SymbolSearchItem> Result { get; set; } = new();
}

public class SymbolSearchItem
{
    public string? Symbol { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? DisplaySymbol { get; set; }
}

// How Finnhub returns data with "Symbol Lookup":
// {
//   "count": 4,
//   "result": [
//     {
//       "description": "APPLE INC",
//       "displaySymbol": "AAPL",
//       "symbol": "AAPL",
//       "type": "Common Stock"
//     },