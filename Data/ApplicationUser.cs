using InvestEasy.Models;
using Microsoft.AspNetCore.Identity;

namespace InvestEasy.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public ICollection<WatchListItemModel> WatchListItems { get; set; } = new List<WatchListItemModel>();
    public ICollection<RiskProfileModel> RiskProfiles { get; set; } = new List<RiskProfileModel>();
    public ICollection<SavingScenarioModel> SavingScenarios { get; set; } = new List<SavingScenarioModel>();
    public ICollection<UserProgressModel> UserProgresses { get; set; } = new List<UserProgressModel>();
}