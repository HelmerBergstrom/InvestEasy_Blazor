using InvestEasy.Models;
using InvestEasy.Services;
using Microsoft.AspNetCore.Components;

namespace InvestEasy.Components.Pages.RiskProfile;

public partial class RiskProfile : ComponentBase
{
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

        savedProfile = await RiskProfileService.GetForUserAsync(userId);
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

        await RiskProfileService.SaveAsync(userId, model);

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