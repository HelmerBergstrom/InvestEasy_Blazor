namespace InvestEasy.Models;

public class IndexQuote
{
    public decimal C { get; set; }   // current price
    public decimal D { get; set; }   // change in price
    public decimal Dp { get; set; }  // percent % change
    public decimal H { get; set; } // hugh price of the day
    public decimal L { get; set; } // low price of the day
    public decimal O { get; set; } // open price of the day
    public decimal Pc { get; set; } // previous close price
}