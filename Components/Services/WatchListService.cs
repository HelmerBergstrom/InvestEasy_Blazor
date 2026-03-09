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
}