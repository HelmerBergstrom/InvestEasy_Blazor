namespace InvestEasy.Services;

public interface ISavingCalculatorService
{
    decimal CalculateFutureValue(decimal monthlyAmount, decimal initialAmount, int years, decimal expectedReturnPercent);

    decimal CalculateTotalInvestment(decimal monthlyAmount, decimal initialAmount, int years);

    decimal CalculateTotalReturn(decimal futureValue, decimal totalInvestment);


}

public class SavingCalculatorService : ISavingCalculatorService
{
    public decimal CalculateFutureValue(decimal monthlyAmount, decimal initialAmount, int years, decimal expectedReturnPercent)
    {
        // Return if yearly return is 0 or negative.
        if (expectedReturnPercent <= 0)
        {
            // Return the value for all invested money.
            // Ex: 10 000kr + (1000kr per month x 12 months x 5 years) = 70 000kr
            return initialAmount + (monthlyAmount * 12 * years);
        }

        // monthlyRate instead of yearly return, for better calculation of compounding.
        var monthlyRate = expectedReturnPercent / 100m / 12m;
        var months = years * 12;

        // Calculate future value for initialAmount. The amount is multiplied with 1 + monthlyRate to get 
        // the increasing per month in percentage, ex. 1,00214 instad of 0,00214 for it to be positive. 
        // Math.Pow raises 1 + monthlyRate with months. (1 + monthlyRate)^months
        decimal futureValueInitial = initialAmount * (decimal)Math.Pow((double)(1 + monthlyRate), months);

        // The same as above, but monthlyAmount increases less and less as closer to months variable we´re getting.
        // Ex: First montlyAmount of 1000kr increases full expectedReturnPercent, second increases expectedReturnPercent 
        // minus 1 month and so on..
        decimal futureValueSavings = monthlyAmount * (((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1) / monthlyRate);

        return futureValueInitial + futureValueSavings;
    }

    public decimal CalculateTotalInvestment(decimal monthlyAmount, decimal initialAmount, int years)
    {
        // if no monthly amount, return initialAmount.
        if (monthlyAmount <= 0)
        {
            return initialAmount;
        }

        // Example: montlyAmount = 1000kr
        // Calculate total monthly savings for a year: monthlyamount * 12
        // Calculate yearly savings for whole period: monthlyAmount * 12 * years
        var totalInvested = initialAmount + (monthlyAmount * 12 * years);

        return totalInvested;
    }

    public decimal CalculateTotalReturn(decimal futureValue, decimal totalInvestment)
    {
        return futureValue - totalInvestment;
    }
}