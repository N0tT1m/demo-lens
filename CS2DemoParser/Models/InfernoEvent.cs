using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("InfernoEvents")]
public class InfernoEvent
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
    
    public int StartTick { get; set; }
    public int? EndTick { get; set; }
    public float StartTime { get; set; }
    public float? EndTime { get; set; }
    public float? Duration { get; set; }
    
    public int InfernoEntityId { get; set; } // Entity index of the inferno
    
    [Required]
    [StringLength(50)]
    public string EventType { get; set; } = null!; // start, spread, extinguish, expire, damage
    
    [StringLength(50)]
    public string? GrenadeType { get; set; } // molotov, incendiary
    
    // Initial position (where grenade landed/fire started)
    [Column(TypeName = "decimal(18,6)")]
    public decimal OriginX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal OriginY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal OriginZ { get; set; }
    
    // Fire spread data
    public float? SpreadRadius { get; set; } // Maximum radius of fire spread
    public float? SpreadArea { get; set; } // Total area covered by fire
    public int? SpreadDirections { get; set; } // Number of directions fire spread
    
    [StringLength(500)]
    public string? SpreadPattern { get; set; } // JSON array of fire positions
    
    // Fire intensity and effectiveness
    public float? MaxIntensity { get; set; } // Peak fire intensity (0-1)
    public float? AverageIntensity { get; set; } // Average intensity over lifetime
    
    public int? DamageDealt { get; set; } // Total damage dealt by this inferno
    public int? PlayersAffected { get; set; } // Number of players damaged
    
    // Tactical analysis
    public bool BlockedPath { get; set; } // Did fire block a pathway?
    public bool ClearedPosition { get; set; } // Did fire clear enemies from position?
    public bool WastedFire { get; set; } // Fire in useless location
    
    [StringLength(100)]
    public string? AreaDenied { get; set; } // Map area that was denied/controlled
    
    [StringLength(100)]
    public string? TacticalPurpose { get; set; } // execute_block, retake_clear, save_stall, etc.
    
    // Environmental factors
    [StringLength(50)]
    public string? SurfaceType { get; set; } // Type of surface fire spread on
    
    public bool HasWaterNearby { get; set; } // Water that could extinguish fire
    public bool WasExtinguished { get; set; } // Manually extinguished (smoke)
    
    public int? ExtinguishedByPlayerId { get; set; } // Player who extinguished
    [ForeignKey("ExtinguishedByPlayerId")]
    public virtual Player? ExtinguishedByPlayer { get; set; }
    
    // Performance metrics
    public float? EffectivenessScore { get; set; } // 0-100, how effective the fire was
    public float? PlacementQuality { get; set; } // 0-100, quality of placement
    public float? TimingScore { get; set; } // 0-100, quality of timing
    
    // Impact on round
    public bool ContributedToRoundWin { get; set; }
    public bool CausedRoundLoss { get; set; } // Poor fire usage
    
    public int? KillsEnabled { get; set; } // Kills that happened because of this fire
    public int? DeathsCaused { get; set; } // Direct deaths from fire damage
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}