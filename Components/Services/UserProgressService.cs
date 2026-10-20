using InvestEasy.Data;
using InvestEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class UserProgressService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public UserProgressService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // gets users progress or creates a starting progress.
    public async Task<UserProgressModel> GetOrCreateProgressAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await GetOrCreateProgressAsync(db, userId);
    }

    // gets userprogress without creating a new progress.
    public async Task<UserProgressModel?> GetProgressAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.UserProgresses.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }

    // marks a level as completed and unlocks next level.
    // Returns false if the level doesn't exist or isn't unlocked yet, so levels can't be skipped.
    public async Task<bool> CompleteLevelAsync(string userId, int levelNumber)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // gets or creates a progress.
        var progress = await GetOrCreateProgressAsync(db, userId);

        if (!IsLevelUnlocked(progress, levelNumber))
        {
            return false;
        }

        // Decides which level is completed.
        switch (levelNumber)
        {
            case 1:
                // markes level 1 as completed and unlocks level 2.
                progress.Level1Completed = true;
                if (progress.CurrentLevel < 2)
                {
                    progress.CurrentLevel = 2;
                }
                break;

            case 2:
                progress.Level2Completed = true;
                if (progress.CurrentLevel < 3)
                {
                    progress.CurrentLevel = 3;
                }
                break;

            case 3:
                progress.Level3Completed = true;
                break;

            default:
                return false;
        }

        await db.SaveChangesAsync();
        return true;
    }

    // checks if level is unlocked.
    public bool IsLevelUnlocked(UserProgressModel progress, int levelNumber)
    {
        return levelNumber >= 1 && levelNumber <= progress.CurrentLevel;
    }

    // checks if level is unlocked for a user, used by the level pages.
    public async Task<bool> IsLevelUnlockedAsync(string userId, int levelNumber)
    {
        var progress = await GetOrCreateProgressAsync(userId);
        return IsLevelUnlocked(progress, levelNumber);
    }

    private static async Task<UserProgressModel> GetOrCreateProgressAsync(ApplicationDbContext db, string userId)
    {
        var progress = await db.UserProgresses.FirstOrDefaultAsync(x => x.UserId == userId);

        if (progress is not null)
        {
            return progress;
        }

        // creates new progress.
        progress = new UserProgressModel
        {
            UserId = userId,
            CurrentLevel = 1,
            Level1Completed = false,
            Level2Completed = false,
            Level3Completed = false
        };

        db.UserProgresses.Add(progress);
        await db.SaveChangesAsync();

        return progress;
    }
}
