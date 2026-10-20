using InvestEasy.Models;
using InvestEasy.Services;

namespace InvestEasy.Tests;

public class RiskProfileServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly RiskProfileService _service;

    public RiskProfileServiceTests()
    {
        _service = new RiskProfileService(_database);
    }

    [Theory]
    [InlineData(1, 0, 1, 0, "Very Low Risk")]
    [InlineData(1, 1, 2, 1, "Low Risk")]
    [InlineData(3, 2, 2, 1, "Moderate Risk")]
    [InlineData(3, 3, 3, 3, "High Risk")]
    public void CalculateProfile_PicksProfileFromTotalScore(int horizon, int drop, int goal, int experience, string expected)
    {
        var model = new RiskProfileModel { TimeHorizon = horizon, MarketDropReaction = drop, InvestmentGoal = goal, ExperienceLevel = experience };

        _service.CalculateProfile(model);

        Assert.Equal(expected, model.ProfileName);
        Assert.Equal(100, model.StocksPercentage + model.FundsPercentage + model.MinimalRiskPercentage);
    }

    [Fact]
    public void CalculateProfile_ShortHorizonIsAlwaysVeryLowRisk()
    {
        var model = new RiskProfileModel { TimeHorizon = 0, MarketDropReaction = 3, InvestmentGoal = 3, ExperienceLevel = 3 };

        _service.CalculateProfile(model);

        Assert.Equal("Very Low Risk", model.ProfileName);
        Assert.Equal(100, model.MinimalRiskPercentage);
    }

    [Fact]
    public async Task SaveAsync_OverwritesTheUsersProfile()
    {
        var userId = _database.AddUser();

        var first = new RiskProfileModel { TimeHorizon = 3, MarketDropReaction = 3, InvestmentGoal = 3, ExperienceLevel = 3 };
        _service.CalculateProfile(first);
        await _service.SaveAsync(userId, first);

        var second = new RiskProfileModel { TimeHorizon = 1, MarketDropReaction = 0, InvestmentGoal = 1, ExperienceLevel = 0 };
        _service.CalculateProfile(second);
        await _service.SaveAsync(userId, second);

        using var db = _database.CreateDbContext();
        var saved = Assert.Single(db.RiskProfiles.Where(r => r.UserId == userId));
        Assert.Equal("Very Low Risk", saved.ProfileName);
    }

    public void Dispose() => _database.Dispose();
}
