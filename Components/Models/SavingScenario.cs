using System.ComponentModel.DataAnnotations;

namespace InvestEasy.Models;

public class SavingScenarioModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Saving scenario name")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 15000)]
    [Display(Name = "Monthly savings")]
    public int MonthlyAmount { get; set; }

    [Required]
    [Display(Name = "Initial amount")]
    public int InitialAmount { get; set; }

    // År för sparandet.
    [Required]
    [Range(1, 50)]
    [Display(Name = "Saving horizon (years)")]
    public int SavingHorizon { get; set; }

    // Årlig procent. Input 7 betyder 7%
    [Required]
    [Range(0, 20)]
    [Display(Name = "Expected yearly return (%)")]
    public int ExpectedReturnPercent { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // FK
    [Required]
    public string UserId { get; set; } = string.Empty;
}