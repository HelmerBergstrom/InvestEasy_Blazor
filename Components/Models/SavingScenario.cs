using System.ComponentModel.DataAnnotations;

namespace InvestEasy.Models;

public class SavingScenarioModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal MonthlyAmount { get; set; }

    [Range(0, 10000000)]
    public decimal InitialAmount { get; set; }

    // År för sparandet.
    [Range(1, 50)]
    public int SavingHorizon { get; set; }

    // Årlig procent. Input 7 betyder 7%
    [Range(0, 20)]
    public decimal ExpectedReturnPercent { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // FK
    [Required]
    public string UserId { get; set; } = string.Empty;
}