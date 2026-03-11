namespace InvestEasy.Services;

using InvestEasy.Models;

public class RiskProfileService
{
    public void CalculateProfile(RiskProfileModel model)
    {
        // if horizon is 0(years) = no risk should be taken.
        if (model.TimeHorizon == 0)
        {
            model.TotalScore = 0;
            model.ProfileName = "Very Low Risk";
            model.FundsPercentage = 0;
            model.StocksPercentage = 0;
            model.MinimalRiskPercentage = 100;
            return;
        }
    }
}