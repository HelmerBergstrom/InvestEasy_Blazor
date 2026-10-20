using InvestEasy.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestEasy.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<SavingScenarioModel> SavingScenarios { get; set; }
    public DbSet<WatchListItemModel> WatchListItems { get; set; }
    public DbSet<UserProgressModel> UserProgresses { get; set; }
    public DbSet<RiskProfileModel> RiskProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // UserId is the foreign key to the user. Cascade delete removes all of a user's data
        // when the account is deleted.
        builder.Entity<SavingScenarioModel>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.SavingScenarios)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WatchListItemModel>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.WatchListItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A stock can only be in a user's watchlist once.
        builder.Entity<WatchListItemModel>()
            .HasIndex(w => new { w.UserId, w.Symbol })
            .IsUnique();

        // A user has one progress and one risk profile.
        builder.Entity<UserProgressModel>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.UserProgresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserProgressModel>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        builder.Entity<RiskProfileModel>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.RiskProfiles)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RiskProfileModel>()
            .HasIndex(r => r.UserId)
            .IsUnique();
    }
}
