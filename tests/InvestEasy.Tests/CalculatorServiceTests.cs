using InvestEasy.Services;

namespace InvestEasy.Tests;

public class CalculatorServiceTests
{
    private readonly CalculatorService _calculator = new();

    [Fact]
    public void ZeroReturn_ValueIsWhatWasDeposited()
    {
        var value = _calculator.CalculateFutureValue(monthlyAmount: 1000, initialAmount: 10000, years: 5, expectedReturnPercent: 0);

        Assert.Equal(70000m, value);
    }

    [Fact]
    public void InitialAmountOnly_GrowsWithMonthlyCompounding()
    {
        var value = _calculator.CalculateFutureValue(monthlyAmount: 0, initialAmount: 10000, years: 1, expectedReturnPercent: 12);

        // 10 000 * 1.01^12
        Assert.Equal(11268.25m, Math.Round(value, 2));
    }

    [Fact]
    public void MonthlySavings_UseTheAnnuityFormula()
    {
        var value = _calculator.CalculateFutureValue(monthlyAmount: 1000, initialAmount: 0, years: 1, expectedReturnPercent: 12);

        // 1000 * (1.01^12 - 1) / 0.01
        Assert.Equal(12682.50m, Math.Round(value, 2));
    }

    [Fact]
    public void LargestAllowedInputs_DoNotOverflow()
    {
        var value = _calculator.CalculateFutureValue(monthlyAmount: 15000, initialAmount: int.MaxValue, years: 50, expectedReturnPercent: 20);

        Assert.True(value > 0);
    }

    [Fact]
    public void TotalInvestment_AndReturn()
    {
        var invested = _calculator.CalculateTotalInvestment(monthlyAmount: 500, initialAmount: 2000, years: 2);

        Assert.Equal(14000m, invested);
        Assert.Equal(1000m, _calculator.CalculateTotalReturn(15000m, invested));
    }
}
