using InvestEasy.Models;
using InvestEasy.Services;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Tests;

// When a user deletes their account, everything they have saved must be deleted too (GDPR).
public class AccountDeletionTests : IDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public async Task DeletingAUser_DeletesAllTheirData()
    {
        var userId = _database.AddUser();
        var otherUserId = _database.AddUser("other@example.com");

        foreach (var id in new[] { userId, otherUserId })
        {
            await new WatchListService(_database).AddAsync(id, "AAPL", "Apple Inc");
            await new UserProgressService(_database).CompleteLevelAsync(id, 1);
            await new SavingScenarioService(_database).CreateScenario(new SavingScenarioModel { Name = "Plan", SavingHorizon = 5, UserId = id });

            var profile = new RiskProfileModel { TimeHorizon = 2, MarketDropReaction = 2, InvestmentGoal = 2, ExperienceLevel = 2 };
            var riskService = new RiskProfileService(_database);
            riskService.CalculateProfile(profile);
            await riskService.SaveAsync(id, profile);
        }

        using (var db = _database.CreateDbContext())
        {
            db.Users.Remove(await db.Users.SingleAsync(u => u.Id == userId));
            await db.SaveChangesAsync();
        }

        using (var db = _database.CreateDbContext())
        {
            Assert.False(await db.WatchListItems.AnyAsync(x => x.UserId == userId));
            Assert.False(await db.UserProgresses.AnyAsync(x => x.UserId == userId));
            Assert.False(await db.SavingScenarios.AnyAsync(x => x.UserId == userId));
            Assert.False(await db.RiskProfiles.AnyAsync(x => x.UserId == userId));

            // The other user's data is untouched.
            Assert.True(await db.WatchListItems.AnyAsync(x => x.UserId == otherUserId));
            Assert.True(await db.RiskProfiles.AnyAsync(x => x.UserId == otherUserId));
        }
    }

    [Fact]
    public async Task DataCantBeSavedForAUserThatDoesntExist()
    {
        var created = new SavingScenarioService(_database);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            created.CreateScenario(new SavingScenarioModel { Name = "Ghost", SavingHorizon = 5, UserId = "no-such-user" }));
    }

    public void Dispose() => _database.Dispose();
}
