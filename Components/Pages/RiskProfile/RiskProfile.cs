using InvestEasy.Data;
using InvestEasy.Models;
using InvestEasy.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Components.Pages.RiskProfile;

public partial class RiskProfile : ComponentBase
{
    [Inject] public ApplicationDbContext DbContext { get; set; } = default!;
    [Inject] public RiskProfileService RiskProfileService { get; set; } = default!;
    [Inject] public CurrentUserService CurrentUserService { get; set; } = default!;

    // values to -1 to force user to answer questions before calculation.
    private RiskProfileModel model = new RiskProfileModel
    {
        TimeHorizon = -1,
        MarketDropReaction = -1,
        InvestmentGoal = -1,
        ExperienceLevel = -1
    };
    private bool hasResult = false;

    private RiskProfileModel? savedProfile;
    private string? saveMessage;

    // boolean for control if question values are 0 or more.
    private bool CanCalculate =>
    model.TimeHorizon >= 0 &&
    model.MarketDropReaction >= 0 &&
    model.InvestmentGoal >= 0 &&
    model.ExperienceLevel >= 0;

    // loading saved risk profile on active component.
    protected override async Task OnInitializedAsync()
    {
        await LoadSavedRiskProfile();
    }

    // Loading saved Risk Profile for user.
    private async Task LoadSavedRiskProfile()
    {
        string? userId = await CurrentUserService.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        savedProfile = await DbContext.RiskProfiles
        .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    // calculates riskprofile via CalculateProfile and user input.
    private async Task CalculateProfile()
    {
        // gets userId
        string? userId = await CurrentUserService.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (!CanCalculate)
        {
            return;
        }

        model.UserId = userId;

        RiskProfileService.CalculateProfile(model);

        hasResult = true;
    }

    // Saves risk profile
    private async Task SaveRiskProfile()
    {
        string? userId = await CurrentUserService.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        model.UserId = userId;

        // gets existing profile if it exists from db.
        RiskProfileModel? existingProfile = await DbContext.RiskProfiles
        .FirstOrDefaultAsync(x => x.UserId == userId);

        // creates profile if user doesn´t have one, overrides current risk profile
        // if profile exists for user.
        if (existingProfile is null)
        {
            model.CreatedUtc = DateTime.UtcNow;
            DbContext.RiskProfiles.Add(model);
        }
        else
        {
            existingProfile.TimeHorizon = model.TimeHorizon;
            existingProfile.MarketDropReaction = model.MarketDropReaction;
            existingProfile.ExperienceLevel = model.ExperienceLevel;
            existingProfile.InvestmentGoal = model.InvestmentGoal;

            existingProfile.TotalScore = model.TotalScore;
            existingProfile.ProfileName = model.ProfileName;

            existingProfile.StocksPercentage = model.StocksPercentage;
            existingProfile.FundsPercentage = model.FundsPercentage;
            existingProfile.MinimalRiskPercentage = model.MinimalRiskPercentage;

            existingProfile.CreatedUtc = DateTime.UtcNow;
        }
        await DbContext.SaveChangesAsync();

        await LoadSavedRiskProfile();
        ResetRiskProfile();
        saveMessage = "Risk profile saved.";
    }

    // resets users answers.
    private void ResetRiskProfile()
    {
        model = new RiskProfileModel
        {
            TimeHorizon = -1,
            MarketDropReaction = -1,
            InvestmentGoal = -1,
            ExperienceLevel = -1
        };

        saveMessage = null;
        hasResult = false;
    }
}