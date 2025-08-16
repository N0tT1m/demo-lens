using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("FireAreas")]
public class FireArea
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
    
    public int? ThrowerPlayerId { get; set; }
    [ForeignKey("ThrowerPlayerId")]
    public virtual Player? ThrowerPlayer { get; set; }
    
    public int GrenadeEntityId { get; set; } // Link to the molotov/incendiary entity
    
    [StringLength(50)]
    public string GrenadeType { get; set; } = null!; // molotov, incgrenade
    
    // Fire area lifecycle
    public int StartTick { get; set; }
    public int? EndTick { get; set; }
    public float StartTime { get; set; }
    public float? EndTime { get; set; }
    public float Duration { get; set; }
    
    // Fire area position and coverage
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterZ { get; set; }
    
    public float MaxRadius { get; set; }
    public float CurrentRadius { get; set; }
    public float Intensity { get; set; } // 0-100, how hot/damaging the fire is
    
    // Spread pattern
    [StringLength(255)]
    public string? SpreadPattern { get; set; } // JSON array of fire spread coordinates
    
    public float SpreadTime { get; set; } // Time for fire to reach full spread
    public float PeakTime { get; set; } // Time at maximum intensity
    public float BurnoutTime { get; set; } // Time to fully extinguish
    
    // Damage tracking
    public float DamagePerSecond { get; set; }
    public float TotalDamageDealt { get; set; }
    public int PlayersAffected { get; set; }
    public int TeammatesAffected { get; set; }
    public int EnemiesAffected { get; set; }
    
    // Area denial effectiveness
    public bool BlocksPath { get; set; }
    public bool ForcesCrouch { get; set; }
    public bool PreventsBombPlant { get; set; }
    public bool PreventsBombDefuse { get; set; }
    
    [StringLength(100)]
    public string? TacticalPurpose { get; set; } // delay, area_denial, flush, damage
    
    // Environmental interaction
    public bool ExtinguishedBySmoke { get; set; }
    public int? ExtinguishingGrenadeId { get; set; }
    public float? ExtinguishTime { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON for additional fire data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}