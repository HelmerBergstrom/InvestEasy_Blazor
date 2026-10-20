using InvestEasy.Services;

namespace InvestEasy.Tests;

public class WatchListServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly WatchListService _service;
    private readonly string _userId;

    public WatchListServiceTests()
    {
        _service = new WatchListService(_database);
        _userId = _database.AddUser();
    }

    [Fact]
    public async Task Add_NormalizesSymbol_AndRejectsDuplicates()
    {
        Assert.Equal(AddToWatchListResult.Added, await _service.AddAsync(_userId, " aapl ", "Apple Inc"));
        Assert.Equal(AddToWatchListResult.AlreadyExists, await _service.AddAsync(_userId, "AAPL", "Apple Inc"));

        var item = Assert.Single(await _service.GetForUserAsync(_userId));
        Assert.Equal("AAPL", item.Symbol);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Add_RejectsEmptySymbol(string symbol)
    {
        Assert.Equal(AddToWatchListResult.Invalid, await _service.AddAsync(_userId, symbol, "Nothing"));
    }

    [Fact]
    public async Task Add_StopsAtTheLimit()
    {
        for (int i = 0; i < WatchListService.MaxItemsPerUser; i++)
        {
            Assert.Equal(AddToWatchListResult.Added, await _service.AddAsync(_userId, $"SYM{i}", $"Stock {i}"));
        }

        Assert.Equal(AddToWatchListResult.LimitReached, await _service.AddAsync(_userId, "ONEMORE", "One more"));
    }

    [Fact]
    public async Task Remove_CantRemoveAnotherUsersItem()
    {
        var otherUserId = _database.AddUser("other@example.com");
        await _service.AddAsync(otherUserId, "MSFT", "Microsoft");
        var otherItem = Assert.Single(await _service.GetForUserAsync(otherUserId));

        Assert.False(await _service.RemoveAsync(_userId, otherItem.Id));
        Assert.Single(await _service.GetForUserAsync(otherUserId));
    }

    [Fact]
    public async Task Get_OnlyReturnsTheUsersOwnItems()
    {
        var otherUserId = _database.AddUser("other@example.com");
        await _service.AddAsync(otherUserId, "MSFT", "Microsoft");

        Assert.Empty(await _service.GetForUserAsync(_userId));
    }

    public void Dispose() => _database.Dispose();
}
