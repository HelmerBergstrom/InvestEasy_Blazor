using InvestEasy.Data;
using InvestEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public enum AddToWatchListResult
{
    Added,
    AlreadyExists,
    LimitReached,
    Invalid
}

public class WatchListService
{
    // Every stock in the watchlist costs one Finnhub call when the page loads, so the list is capped.
    public const int MaxItemsPerUser = 20;

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public WatchListService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<WatchListItemModel>> GetForUserAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.WatchListItems
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedUtc)
            .ToListAsync();
    }

    // Add stock to watchlist.
    public async Task<AddToWatchListResult> AddAsync(string userId, string symbol, string name, string? type = null)
    {
        // Normalize symbol before saving to db, helps validation for duplicates.
        symbol = symbol.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(symbol) || symbol.Length > 30)
        {
            return AddToWatchListResult.Invalid;
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        // checks if stock already exists in db. Checks user + stock symbol.
        var exists = await db.WatchListItems.AnyAsync(item => item.UserId == userId && item.Symbol == symbol);

        if (exists)
        {
            return AddToWatchListResult.AlreadyExists;
        }

        var count = await db.WatchListItems.CountAsync(item => item.UserId == userId);

        if (count >= MaxItemsPerUser)
        {
            return AddToWatchListResult.LimitReached;
        }

        var item = new WatchListItemModel
        {
            UserId = userId,
            Symbol = symbol,
            Name = name.Length > 200 ? name[..200] : name,
            Type = type,
            CreatedUtc = DateTime.UtcNow
        };

        db.WatchListItems.Add(item);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The unique index on (UserId, Symbol) stopped a duplicate, ex. after a double click.
            return AddToWatchListResult.AlreadyExists;
        }

        return AddToWatchListResult.Added;
    }

    // remove stock from watchlist.
    public async Task<bool> RemoveAsync(string userId, int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // gets specific watchlist item from db from correct user.
        var item = await db.WatchListItems.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id);

        if (item is null)
        {
            return false;
        }

        db.WatchListItems.Remove(item);
        await db.SaveChangesAsync();

        return true;
    }
}
