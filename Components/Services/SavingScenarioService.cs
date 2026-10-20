using InvestEasy.Models;
using InvestEasy.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class SavingScenarioService
{
    // Limits that stop a single user from filling the database.
    public const int MaxScenariosPerUser = 50;
    public const int MaxNameLength = 60;

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public SavingScenarioService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Method to save a scenario to database. Returns null if the user has reached the limit.
    public async Task<SavingScenarioModel?> CreateScenario(SavingScenarioModel scenario)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var count = await db.SavingScenarios.CountAsync(s => s.UserId == scenario.UserId);
        if (count >= MaxScenariosPerUser)
        {
            return null;
        }

        // new scenario
        var entity = new SavingScenarioModel
        {
            Name = CleanName(scenario.Name),
            MonthlyAmount = scenario.MonthlyAmount,
            InitialAmount = scenario.InitialAmount,
            SavingHorizon = scenario.SavingHorizon,
            ExpectedReturnPercent = scenario.ExpectedReturnPercent,
            CreatedDate = DateTime.UtcNow,
            UserId = scenario.UserId
        };

        // Save to database
        db.SavingScenarios.Add(entity);
        await db.SaveChangesAsync();

        return entity;
    }

    // Method to get all scenarios for a specific user.
    public async Task<List<SavingScenarioModel>> GetAllScenariosForUser(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.SavingScenarios
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    // Method to get one scenario for a specific user.
    public async Task<SavingScenarioModel?> GetByIdAsync(int id, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.SavingScenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    // Method to update a scenario to database.
    public async Task<bool> UpdateScenario(int id, string userId, SavingScenarioModel updated)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // gets specific scenario.
        var entity = await db.SavingScenarios
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (entity is null)
            return false;

        // updates values.
        entity.Name = CleanName(updated.Name);
        entity.MonthlyAmount = updated.MonthlyAmount;
        entity.InitialAmount = updated.InitialAmount;
        entity.SavingHorizon = updated.SavingHorizon;
        entity.ExpectedReturnPercent = updated.ExpectedReturnPercent;

        // saves to db.
        await db.SaveChangesAsync();
        return true;
    }

    // Method to delete one scenario for a specific user. Returns true if successful.
    public async Task<bool> DeleteScenario(int id, string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var entity = await db.SavingScenarios
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (entity is null)
        {
            return false;
        }

        db.SavingScenarios.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    // Trims the name and cuts it to the max length. The input has a maxlength, but that is only enforced in the browser.
    private static string CleanName(string? name)
    {
        var trimmed = name?.Trim() ?? "";
        return trimmed.Length > MaxNameLength ? trimmed[..MaxNameLength] : trimmed;
    }
}
