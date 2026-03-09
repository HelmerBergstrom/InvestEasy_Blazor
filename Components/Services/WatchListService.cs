using InvestEasy.Data;
using InvestEasy.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Services;

public class WatchListService
{
    private readonly ApplicationDbContext _db;

    public WatchListService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<WatchListItemModel>> GetForUserAsync(string userId)
    {
        return await _db.WatchListItems.Where(item => item.UserId == userId).ToListAsync();
    }

    // Add stock to watchlist. 
    public async Task<bool> AddAsync(string userId, string symbol, string name, string? type = null)
    {
        // Normalize symbol before saving to db, helps validation for duplicates.
        symbol = symbol.Trim().ToUpperInvariant();

        // checks if stock already exists in db. Checks user + stock symbol.
        var exists = await _db.WatchListItems.AnyAsync(item => item.UserId == userId && item.Symbol == symbol);

        if (exists)
        {
            return false;
        }
        ;

        var item = new WatchListItemModel
        {
            UserId = userId,
            Symbol = symbol,
            Name = name,
            Type = type,
            CreatedUtc = DateTime.UtcNow
        };

        _db.WatchListItems.Add(item);
        await _db.SaveChangesAsync();

        return true;
    }

    // remove stock from watchlist.
    public async Task<bool> RemoveAsync(string userId, int id)
    {
        // gets specific watchlist item from db from correct user.
        var item = await _db.WatchListItems.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id);

        if (item is null)
        {
            return false;
        }
        ;

        _db.WatchListItems.Remove(item);
        await _db.SaveChangesAsync();

        return true;
    }
}