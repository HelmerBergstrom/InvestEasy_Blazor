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

        model.TotalScore =
        model.TimeHorizon +
        model.MarketDropReaction +
        model.InvestmentGoal +
        model.ExperienceLevel;

        if (model.TotalScore <= 3)
        {
            model.ProfileName = "Very Low Risk";
            model.StocksPercentage = 0;
            model.FundsPercentage = 0;
            model.MinimalRiskPercentage = 100;
        }
        else if (model.TotalScore <= 6)
        {
            model.ProfileName = "Low Risk";
            model.StocksPercentage = 20;
            model.FundsPercentage = 50;
            model.MinimalRiskPercentage = 30;
        }
        else if (model.TotalScore <= 9)
        {
            model.ProfileName = "Moderate Risk";
            model.StocksPercentage = 30;
            model.FundsPercentage = 70;
            model.MinimalRiskPercentage = 0;
        }
        else
        {
            model.ProfileName = "High Risk";
            model.StocksPercentage = 70;
            model.FundsPercentage = 30;
            model.MinimalRiskPercentage = 0;
        }
    }
}