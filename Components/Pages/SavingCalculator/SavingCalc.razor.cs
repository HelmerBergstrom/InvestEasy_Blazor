using System.Globalization;
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
    protected string? editingScenarioName;

    // Limits for the inputs, same as the Range-attributes in SavingScenarioModel.
    protected const int MonthlyMin = 0;
    protected const int MonthlyMax = 15000;
    protected const int YearsMin = 1;
    protected const int YearsMax = 50;
    protected const int ReturnMin = 0;
    protected const int ReturnMax = 20;

    // Quick choices for expected annual return.
    protected record ReturnPreset(string Label, int Percent);
    protected readonly List<ReturnPreset> returnPresets = new()
    {
        new("Cautious", 3),
        new("Balanced", 5),
        new("Growth", 8)
    };

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

    protected decimal futureValue;
    protected decimal totalInvestment;
    protected decimal totalReturn;

    // Value for each year, used by GrowthChart.
    protected List<GrowthPoint> growth = new();

    // First year where returns are larger than the invested amount, null if never.
    protected int? breakEvenYear;

    // Share of the final value that comes from returns.
    protected int ReturnShare => futureValue > 0 ? (int)Math.Round(totalReturn / futureValue * 100) : 0;

    // gets all saved scenarios when component is ready to start
    protected override async Task OnInitializedAsync()
    {
        Recalculate();

        var userId = await CurrentUserService.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
            return;

        savedScenarios = await ScenarioService.GetAllScenariosForUser(userId);
    }

    // Calculates users input with the model above and through the calculations in CalculatorService.functions.
    // Runs every time an input changes, so the result is always up to date.
    protected void Recalculate()
    {
        // Keeps values within limits, ex. if the user types a too large number.
        model.InitialAmount = Math.Max(0, model.InitialAmount);
        model.MonthlyAmount = Math.Clamp(model.MonthlyAmount, MonthlyMin, MonthlyMax);
        model.SavingHorizon = Math.Clamp(model.SavingHorizon, YearsMin, YearsMax);
        model.ExpectedReturnPercent = Math.Clamp(model.ExpectedReturnPercent, ReturnMin, ReturnMax);

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

        // Calculates the value at the end of every year for the chart.
        growth = Enumerable.Range(1, model.SavingHorizon)
            .Select(year => new GrowthPoint(
                year,
                CalculatorService.CalculateTotalInvestment(model.MonthlyAmount, model.InitialAmount, year),
                CalculatorService.CalculateFutureValue(model.MonthlyAmount, model.InitialAmount, year, model.ExpectedReturnPercent)))
            .ToList();

        breakEvenYear = growth.FirstOrDefault(p => p.Invested > 0 && p.Returns > p.Invested)?.Year;
    }

    protected void SetReturn(int percent)
    {
        model.ExpectedReturnPercent = percent;
        Recalculate();
    }

    // How much of a slider track to fill, ex. "35%".
    protected static string FillPercent(int value, int min, int max) =>
        ((double)(value - min) / (max - min) * 100).ToString("0.#", CultureInfo.InvariantCulture) + "%";

    // CSS values must use "." as decimal separator regardless of the server culture.
    protected static string Invariant(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    protected void ClearCalculation()
    {
        saveMessage = null;
        saveError = null;

        model.Name = "";
        model.MonthlyAmount = 1000;
        model.InitialAmount = 1000;
        model.SavingHorizon = 5;
        model.ExpectedReturnPercent = 7;

        Recalculate();
    }

    // Validates and sends calculation/scenario to SavingScenarioService
    // for database-handling.
    protected async Task SaveScenario()
    {
        saveError = null;
        saveMessage = null;

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

        // Resets the form but keeps the message visible.
        ClearCalculation();
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
        editingScenarioName = scenario.Name;

        // Sets form values to scenario values
        model.Name = scenario.Name;
        model.MonthlyAmount = scenario.MonthlyAmount;
        model.InitialAmount = scenario.InitialAmount;
        model.SavingHorizon = scenario.SavingHorizon;
        model.ExpectedReturnPercent = scenario.ExpectedReturnPercent;

        Recalculate();
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

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            saveError = "Please give the scenario a name before saving.";
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

        editingScenarioId = null;
        editingScenarioName = null;

        ClearCalculation();
        saveMessage = "Scenario updated.";
    }

    // Cancels edit-mode and goes back to normal form
    protected void CancelEdit()
    {
        editingScenarioId = null;
        editingScenarioName = null;
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