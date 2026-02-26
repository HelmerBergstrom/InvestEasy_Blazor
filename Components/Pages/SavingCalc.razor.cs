using Microsoft.AspNetCore.Components;
using InvestEasy.Models;
using InvestEasy.Services;

namespace InvestEasy.Components.Pages;

public partial class SavingCalc
{
    [Inject] public ICalculatorService CalculatorService { get; set; } = default!;
    [Inject] public SavingScenarioService ScenarioService { get; set; } = default!;
    [Inject] public CurrentUserService CurrentUserService { get; set; } = default!;

    // Saved scenarios
    protected List<SavingScenarioModel> savedScenarios = new();
    protected string? saveError;
    protected string? saveMessage;

    // Model for the EditForm. Saves and binds the input.
    protected SavingScenarioModel model = new()
    {
        Name = "",
        MonthlyAmount = 1000m,
        InitialAmount = 1000m,
        SavingHorizon = 5,
        ExpectedReturnPercent = 7m,
        UserId = "placeholder"
    };

    protected bool hasCalculated;
    protected decimal futureValue;
    protected decimal totalInvestment;
    protected decimal totalReturn;

    // gets all saved scenarios when component is ready to start
    protected override async Task OnInitializedAsync()
    {
        var userId = await CurrentUserService.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return;

        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);
    }

    // method to save a scenario
    protected async Task SaveScenario()
    {
        saveError = null;
        saveMessage = null;

        if (!hasCalculated)
        {
            saveError = "Use calculation before saving!";
            return;
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            saveError = "Please give the scenario a name before saving.";
            return;
        }

        var userId = await CurrentUserService.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            saveError = "Couldn´t identify user. Log in again.";
            return;
        }

        // Linking scenario to user
        model.UserId = userId;

        // CREATE to database
        await ScenarioService.CreateScenario(model);

        // GET lastest list
        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);

        saveMessage = "Scenario saved.";
    }

    // Calculates users input with the model above and through the calculations in CalculatorService.functions
    protected void Calculate()
    {
        hasCalculated = true;

        futureValue = CalculatorService.CalculateFutureValue(
            model.MonthlyAmount,
            model.InitialAmount,
            model.SavingHorizon,
            model.ExpectedReturnPercent);

        totalInvestment = CalculatorService.CalculateTotalInvestment(
            model.MonthlyAmount,
            model.InitialAmount,
            model.SavingHorizon);

        totalReturn = CalculatorService.CalculateTotalReturn(
            futureValue,
            totalInvestment);
    }
}