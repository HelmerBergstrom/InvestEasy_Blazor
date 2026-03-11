using Microsoft.AspNetCore.Components;
using InvestEasy.Models;
using InvestEasy.Services;

namespace InvestEasy.Components.Pages.SavingCalculator;

public partial class SavingCalc
{
    [Inject] public ICalculatorService CalculatorService { get; set; } = default!;
    [Inject] public SavingScenarioService ScenarioService { get; set; } = default!;
    [Inject] public CurrentUserService CurrentUserService { get; set; } = default!;

    // Saved scenarios
    protected List<SavingScenarioModel> savedScenarios = new();
    protected string? saveError;
    protected string? saveMessage;
    protected int? editingScenarioId = null;

    // Model for the EditForm. Saves and binds the input.
    protected SavingScenarioModel model = new()
    {
        Name = "",
        MonthlyAmount = 1000,
        InitialAmount = 1000,
        SavingHorizon = 5,
        ExpectedReturnPercent = 7,
        UserId = "placeholder"
    };

    protected bool hasCalculated;
    protected decimal futureValue;
    protected decimal totalInvestment;
    protected decimal totalReturn;

    // gets all saved scenarios when component is ready to start
    protected override async Task OnInitializedAsync()
    {
        var userId = await CurrentUserService.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return;

        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);
    }

    // Calculates users input with the model above and through the calculations in CalculatorService.functions
    protected void Calculate()
    {
        hasCalculated = true;
        saveMessage = null;

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

    protected void ClearCalculation()
    {
        hasCalculated = false;

        saveMessage = null;
        saveError = null;

        futureValue = 0;
        totalInvestment = 0;
        totalReturn = 0;


        model.Name = "";
        model.MonthlyAmount = 1000;
        model.InitialAmount = 1000;
        model.SavingHorizon = 5;
        model.ExpectedReturnPercent = 7;
    }

    // Validates and sends calculation/scenario to SavingScenarioService
    // for database-handling.
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

        var userId = await CurrentUserService.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            saveError = "Couldn´t identify user. Log in again.";
            return;
        }

        // Linking scenario to user
        model.UserId = userId;

        // Send model to method in SavingScenarioService to CREATE to database.
        await ScenarioService.CreateScenario(model);

        // GET lastest list
        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);

        saveMessage = "Scenario saved.";
    }

    // starting "edit-mode". 
    protected void StartEditScenario(int id)
    {
        saveError = null;
        saveMessage = null;

        var scenario = savedScenarios.FirstOrDefault(s => s.Id == id);
        if (scenario is null)
        {
            saveError = "Scenario not found.";
            return;
        }

        editingScenarioId = id;

        // Sets form values to scenario values
        model.Name = scenario.Name;
        model.MonthlyAmount = scenario.MonthlyAmount;
        model.InitialAmount = scenario.InitialAmount;
        model.SavingHorizon = scenario.SavingHorizon;
        model.ExpectedReturnPercent = scenario.ExpectedReturnPercent;

        hasCalculated = false; // Goes back to form-mode.
    }

    // Method to update a specific scenario.
    protected async Task UpdateScenario()
    {
        saveError = null;
        saveMessage = null;

        if (editingScenarioId is null)
        {
            saveError = "No scenario selected for editing.";
            return;
        }

        var userId = await CurrentUserService.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            saveError = "Couldn't identify user. Please log in again.";
            return;
        }

        // sets updated scenario to database.
        var updated = await ScenarioService.UpdateScenario(
            editingScenarioId.Value,
            userId,
            model);

        if (!updated)
        {
            saveError = "Couldn't update scenario.";
            return;
        }

        // gets saved scenarios again.
        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);

        saveMessage = "Scenario updated.";
        editingScenarioId = null;
    }

    // Cancels edit-mode and goes back to normal form
    protected void CancelEdit()
    {
        editingScenarioId = null;
        ClearCalculation();
    }

    // Method to delete a scenario. 
    protected async Task DeleteScenario(int scenarioId)
    {
        saveError = null;
        saveMessage = null;

        // Gets user and validates user.
        var userId = await CurrentUserService.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            saveError = "Couldn´t identify user. Please log in again.";
            return;
        }

        // Send model to method in SavingScenarioService to DELETE from database.
        var deleted = await ScenarioService.DeleteScenario(scenarioId, userId);
        if (!deleted)
        {
            saveError = "Couldn´t delete the scenario.";
            return;
        }

        // Loading lis again
        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);
        saveMessage = "Scenario deleted.";
    }
}