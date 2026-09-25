namespace InvestEasy.Components.Pages.SavingCalculator;

// Value of a saving scenario at the end of a given year.
public record GrowthPoint(int Year, decimal Invested, decimal Value)
{
    public decimal Returns => Math.Max(0, Value - Invested);
}
