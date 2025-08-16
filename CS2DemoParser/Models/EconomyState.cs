using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EconomyStates")]
public class EconomyState
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DemoFileId { get; set; }
    [ForeignKey("DemoFileId")]
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    public int? RoundId { get; set; }
    [ForeignKey("RoundId")]
    public virtual Round? Round { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Phase { get; set; } = null!; // freeze_time, buy_time, round_active, round_end
    
    // Money tracking
    public int TotalMoney { get; set; }
    public int AverageMoney { get; set; }
    public int MaxMoney { get; set; }
    public int MinMoney { get; set; }
    public int MoneySpent { get; set; }
    public int MoneyLost { get; set; } // From deaths
    
    // Round type classification
    [StringLength(50)]
    public string RoundType { get; set; } = null!; // eco, force, full_buy, save, anti_eco
    
    public float BuyPercentage { get; set; } // 0-100, percentage of money spent
    public int PlayersCanFullBuy { get; set; } // Number of players who can afford full buy
    public int PlayersOnEco { get; set; } // Number of players saving/eco
    
    // Equipment investment
    public int TotalWeaponValue { get; set; }
    public int TotalUtilityValue { get; set; }
    public int TotalArmorValue { get; set; }
    public int TotalDefuseKitValue { get; set; }
    
    // Economic efficiency metrics
    public float DamagePerDollar { get; set; } // Damage dealt per dollar spent
    public float KillsPerDollar { get; set; } // Kills per dollar spent
    public float UtilityEfficiency { get; set; } // Utility impact per dollar
    
    // Predictive metrics
    public int NextRoundMoney { get; set; } // Predicted money for next round
    public bool CanFullBuyNextRound { get; set; }
    public int RoundsUntilFullBuy { get; set; } // Rounds needed to afford full buy
    
    // Historical context
    public int ConsecutiveLosses { get; set; }
    public int ConsecutiveWins { get; set; }
    public int LossBonus { get; set; } // Current loss bonus multiplier
    
    [StringLength(255)]
    public string? EconomicPressure { get; set; } // Description of economic situation
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}