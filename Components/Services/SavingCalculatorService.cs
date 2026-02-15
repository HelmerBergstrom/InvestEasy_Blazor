namespace InvestEasy.Services;

public interface ISavingCalculatorService
{
    decimal CalculateFutureValue(decimal monthlyAmount, decimal initialAmount, int years, decimal expectedReturnPercent);

    decimal CalculateTotalInvested(decimal monthlyAmount, decimal initialAmount, int years);

    decimal CalculateTotalReturns(decimal futureValue, decimal totalContributions);


}

public class SavingCalculatorService : ISavingCalculatorService
{
    public decimal CalculateFutureValue(decimal monthlyAmount, decimal initialAmount, int years, decimal expectedReturnPercent)
    {
        if (expectedReturnPercent <= 0)
        {
            return initialAmount + (monthlyAmount * 12 * years);
        }


        throw new NotImplementedException();
    }

    public decimal CalculateTotalInvested(decimal monthlyAmount, decimal initialAmount, int years)
    {
        throw new NotImplementedException();
    }

    public decimal CalculateTotalReturns(decimal futureValue, decimal totalContributions)
    {
        throw new NotImplementedException();
    }
}