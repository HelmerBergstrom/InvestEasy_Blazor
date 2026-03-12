namespace InvestEasy.Models;

public class RiskProfileModel
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    // User answers
    public int TimeHorizon { get; set; }
    public int MarketDropReaction { get; set; }
    public int InvestmentGoal { get; set; }
    public int ExperienceLevel { get; set; }

    // Calculated result
    public int TotalScore { get; set; }
    public string ProfileName { get; set; } = "";

    public int StocksPercentage { get; set; }
    public int FundsPercentage { get; set; }

    // minimal risk = basic saving account etc.
    public int MinimalRiskPercentage { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}