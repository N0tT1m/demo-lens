using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("TeamStates")]
public class TeamState
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
    public string Team { get; set; } = null!; // CT, T
    
    public int RoundNumber { get; set; }
    
    // Economic state
    public int TotalMoney { get; set; }
    public int AverageMoney { get; set; }
    public int TotalEquipmentValue { get; set; }
    public int LivingPlayers { get; set; }
    public int PlayersWithArmor { get; set; }
    public int PlayersWithHelmet { get; set; }
    public int PlayersWithDefuseKit { get; set; }
    
    // Weapon distribution
    public int RifleCount { get; set; }
    public int PistolCount { get; set; }
    public int SniperCount { get; set; }
    public int SMGCount { get; set; }
    public int ShotgunCount { get; set; }
    
    // Utility distribution
    public int HEGrenadeCount { get; set; }
    public int FlashbangCount { get; set; }
    public int SmokegrenadeCount { get; set; }
    public int MolotovCount { get; set; }
    public int DecoyCount { get; set; }
    
    // Tactical positioning
    [StringLength(100)]
    public string? PrimaryArea { get; set; } // Primary area of focus
    [StringLength(100)]
    public string? SecondaryArea { get; set; } // Secondary area coverage
    public float TeamSpread { get; set; } // How spread out the team is
    public bool IsStacked { get; set; } // If team is stacked on one site
    public bool IsRotating { get; set; } // If team is in rotation
    
    // Round state context
    public bool IsSaveRound { get; set; }
    public bool IsForceRound { get; set; }
    public bool IsEcoRound { get; set; }
    public bool IsFullBuyRound { get; set; }
    public bool IsAntiEcoRound { get; set; }
    
    // Team coordination indicators
    public float TeamCohesion { get; set; } // 0-100, how close together team is playing
    public float TradeKillPotential { get; set; } // 0-100, likelihood of trade kills
    public float SiteControl { get; set; } // 0-100, percentage of map controlled
    
    [StringLength(255)]
    public string? TacticalNotes { get; set; } // Additional tactical observations
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}