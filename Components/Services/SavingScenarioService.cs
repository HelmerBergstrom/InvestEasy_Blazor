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
}
