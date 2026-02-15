using InvestEasy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SavingScenarioModel> SavingScenarios { get; set; }
    public DbSet<WatchListItemModel> WatchListItems { get; set; }
    public DbSet<RiskProfileModel> RiskProfiles { get; set; }
    public DbSet<UserProgressModel> UserProgresses { get; set; }
}
