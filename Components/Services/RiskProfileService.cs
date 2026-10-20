using InvestEasy.Data;
using InvestEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class RiskProfileService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public RiskProfileService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Gets the saved risk profile for a user, null if the user hasn't saved one.
    public async Task<RiskProfileModel?> GetForUserAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.RiskProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    // Saves a calculated profile. A user has one profile, so an existing profile is overwritten.
    public Task SaveAsync(string userId, RiskProfileModel model) => SaveAsync(userId, model, retryOnConflict: true);

    private async Task SaveAsync(string userId, RiskProfileModel model, bool retryOnConflict)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        RiskProfileModel? profile = await db.RiskProfiles.FirstOrDefaultAsync(x => x.UserId == userId);

        if (profile is null)
        {
            profile = new RiskProfileModel { UserId = userId };
            db.RiskProfiles.Add(profile);
        }

        profile.TimeHorizon = model.TimeHorizon;
        profile.MarketDropReaction = model.MarketDropReaction;
        profile.ExperienceLevel = model.ExperienceLevel;
        profile.InvestmentGoal = model.InvestmentGoal;

        profile.TotalScore = model.TotalScore;
        profile.ProfileName = model.ProfileName;

        profile.StocksPercentage = model.StocksPercentage;
        profile.FundsPercentage = model.FundsPercentage;
        profile.MinimalRiskPercentage = model.MinimalRiskPercentage;

        profile.CreatedUtc = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException) when (retryOnConflict && db.Entry(profile).State == EntityState.Added)
        {
            // Another request saved a profile at the same time (UserId is unique). Saves once more as an update.
            await SaveAsync(userId, model, retryOnConflict: false);
        }
    }

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