using InvestEasy.Data;
using InvestEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class UserProgressService
{
    private readonly ApplicationDbContext _db;

    public UserProgressService(ApplicationDbContext db)
    {
        _db = db;
    }

    // gets users progress or creates a starting progress.
    public async Task<UserProgressModel> GetOrCreateProgressAsync(string userId)
    {
        var progress = await _db.UserProgresses.FirstOrDefaultAsync(x => x.UserId == userId);

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

        _db.UserProgresses.Add(progress);
        await _db.SaveChangesAsync();

        return progress;
    }

    // gets userprogress without creating a new progress.
    public async Task<UserProgressModel?> GetProgressAsync(string userId)
    {
        return await _db.UserProgresses.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    // marks a level as completed and unlocks next level.
    public async Task<bool> CompleteLevelAsync(string userId, int levelNumber)
    {
        // gets or creates a progress.
        var progress = await GetOrCreateProgressAsync(userId);

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

        await _db.SaveChangesAsync();
        return true;
    }

    // checks if level is unlocked.
    public bool IsLevelUnlocked(UserProgressModel progress, int levelNumber)
    {
        return levelNumber <= progress.CurrentLevel;
    }
}