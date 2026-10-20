using InvestEasy.Services;

namespace InvestEasy.Tests;

public class UserProgressServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly UserProgressService _service;
    private readonly string _userId;

    public UserProgressServiceTests()
    {
        _service = new UserProgressService(_database);
        _userId = _database.AddUser();
    }

    [Fact]
    public async Task NewUser_OnlyLevel1IsUnlocked()
    {
        Assert.True(await _service.IsLevelUnlockedAsync(_userId, 1));
        Assert.False(await _service.IsLevelUnlockedAsync(_userId, 2));
        Assert.False(await _service.IsLevelUnlockedAsync(_userId, 3));
    }

    [Fact]
    public async Task LockedLevel_CantBeCompleted()
    {
        Assert.False(await _service.CompleteLevelAsync(_userId, 3));

        var progress = await _service.GetOrCreateProgressAsync(_userId);
        Assert.False(progress.Level3Completed);
        Assert.Equal(1, progress.CurrentLevel);
    }

    [Fact]
    public async Task CompletingLevels_UnlocksTheNextOne()
    {
        Assert.True(await _service.CompleteLevelAsync(_userId, 1));
        Assert.True(await _service.IsLevelUnlockedAsync(_userId, 2));

        Assert.True(await _service.CompleteLevelAsync(_userId, 2));
        Assert.True(await _service.CompleteLevelAsync(_userId, 3));

        var progress = await _service.GetOrCreateProgressAsync(_userId);
        Assert.True(progress.Level1Completed && progress.Level2Completed && progress.Level3Completed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public async Task UnknownLevel_IsRejected(int level)
    {
        Assert.False(await _service.CompleteLevelAsync(_userId, level));
    }

    [Fact]
    public async Task GetOrCreate_CalledAgain_ReusesTheProgress()
    {
        for (int i = 0; i < 3; i++)
        {
            await _service.GetOrCreateProgressAsync(_userId);
        }

        using var db = _database.CreateDbContext();
        Assert.Single(db.UserProgresses.Where(p => p.UserId == _userId));
    }

    public void Dispose() => _database.Dispose();
}
