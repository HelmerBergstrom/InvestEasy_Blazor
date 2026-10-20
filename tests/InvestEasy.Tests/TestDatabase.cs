using InvestEasy.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace InvestEasy.Tests;

// An in-memory SQLite database with the real schema. SQLite (unlike the EF in-memory provider)
// enforces foreign keys and unique indexes, so the tests catch the same errors as production.
public sealed class TestDatabase : IDbContextFactory<ApplicationDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public TestDatabase()
    {
        // The database lives as long as the connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            // The app sets the Identity schema version (with passkeys) through IdentityOptions, which this
            // context doesn't get, so EF sees a small difference to the migrations. The tables are the same.
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        using var db = CreateDbContext();
        db.Database.Migrate();
    }

    public ApplicationDbContext CreateDbContext() => new(_options);

    // Creates a user, since every user-owned row needs an existing user.
    public string AddUser(string email = "user@example.com")
    {
        using var db = CreateDbContext();
        var user = new ApplicationUser { UserName = email, Email = email };
        db.Users.Add(user);
        db.SaveChanges();
        return user.Id;
    }

    public void Dispose() => _connection.Dispose();
}
