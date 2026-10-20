using InvestEasy.Models;
using InvestEasy.Services;

namespace InvestEasy.Tests;

public class SavingScenarioServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly SavingScenarioService _service;
    private readonly string _userId;

    public SavingScenarioServiceTests()
    {
        _service = new SavingScenarioService(_database);
        _userId = _database.AddUser();
    }

    private SavingScenarioModel Scenario(string name = "Pension") => new()
    {
        Name = name,
        MonthlyAmount = 1000,
        InitialAmount = 5000,
        SavingHorizon = 10,
        ExpectedReturnPercent = 7,
        UserId = _userId
    };

    [Fact]
    public async Task Create_TrimsAndShortensTheName()
    {
        var created = await _service.CreateScenario(Scenario("  " + new string('a', 100) + "  "));

        Assert.NotNull(created);
        Assert.Equal(SavingScenarioService.MaxNameLength, created.Name.Length);
    }

    [Fact]
    public async Task Create_StopsAtTheLimit()
    {
        for (int i = 0; i < SavingScenarioService.MaxScenariosPerUser; i++)
        {
            Assert.NotNull(await _service.CreateScenario(Scenario($"Plan {i}")));
        }

        Assert.Null(await _service.CreateScenario(Scenario("One more")));
    }

    [Fact]
    public async Task UpdateAndDelete_OnlyWorkForTheOwner()
    {
        var created = await _service.CreateScenario(Scenario());
        var otherUserId = _database.AddUser("other@example.com");

        Assert.False(await _service.UpdateScenario(created!.Id, otherUserId, Scenario("Hacked")));
        Assert.False(await _service.DeleteScenario(created.Id, otherUserId));

        var stored = await _service.GetByIdAsync(created.Id, _userId);
        Assert.Equal("Pension", stored!.Name);
    }

    [Fact]
    public async Task Update_ChangesTheValues()
    {
        var created = await _service.CreateScenario(Scenario());
        var updated = Scenario("House");
        updated.MonthlyAmount = 2500;

        Assert.True(await _service.UpdateScenario(created!.Id, _userId, updated));

        var stored = await _service.GetByIdAsync(created.Id, _userId);
        Assert.Equal("House", stored!.Name);
        Assert.Equal(2500, stored.MonthlyAmount);
    }

    public void Dispose() => _database.Dispose();
}
