using InvestEasy.Models;
using InvestEasy.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class SavingScenarioService
{
    private readonly ApplicationDbContext dbContext;

    public SavingScenarioService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // Method to save a scenario to database.
    public async Task<SavingScenarioModel> CreateScenario(SavingScenarioModel scenario)
    {
        // new scenario
        var entity = new SavingScenarioModel
        {
            Name = scenario.Name?.Trim() ?? "",
            MonthlyAmount = scenario.MonthlyAmount,
            InitialAmount = scenario.InitialAmount,
            SavingHorizon = scenario.SavingHorizon,
            ExpectedReturnPercent = scenario.ExpectedReturnPercent,
            CreatedDate = DateTime.UtcNow,
            UserId = scenario.UserId
        };

        // Save to database
        dbContext.SavingScenarios.Add(entity);
        await dbContext.SaveChangesAsync();

        return entity;
    }

    // Method to get all scenarios for a specific user.
    public async Task<List<SavingScenarioModel>> GetAllScenariosForUser(string userId)
    {
        return await dbContext.SavingScenarios
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync();
    }

    // Method to get one scenario for a specific user.
    public async Task<SavingScenarioModel?> GetByIdAsync(int id, string userId)
    {
        return await dbContext.SavingScenarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    // Method to update a scenario to database.
    public async Task<bool> UpdateScenario(int id, string userId, SavingScenarioModel updated)
    {
        // gets specific scenario.
        var entity = await dbContext.SavingScenarios
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (entity is null)
            return false;

        // updates values.
        entity.Name = updated.Name?.Trim() ?? "";
        entity.MonthlyAmount = updated.MonthlyAmount;
        entity.InitialAmount = updated.InitialAmount;
        entity.SavingHorizon = updated.SavingHorizon;
        entity.ExpectedReturnPercent = updated.ExpectedReturnPercent;

        // saves to db.
        await dbContext.SaveChangesAsync();
        return true;
    }

    // Method to delete one scenario for a specific user. Returns true if successful.
    public async Task<bool> DeleteScenario(int id, string userId)
    {
        var entity = await dbContext.SavingScenarios
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (entity is null)
        {
            return false;
        }

        dbContext.SavingScenarios.Remove(entity);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
