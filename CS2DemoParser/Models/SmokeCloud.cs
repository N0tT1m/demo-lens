using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("SmokeClouds")]
public class SmokeCloud
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
    
    public int GrenadeEntityId { get; set; } // Link to the smoke grenade entity
    
    // Smoke cloud lifecycle
    public int StartTick { get; set; }
    public int? EndTick { get; set; }
    public float StartTime { get; set; }
    public float? EndTime { get; set; }
    public float Duration { get; set; }
    
    // Smoke cloud position and area
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterZ { get; set; }
    
    public float MaxRadius { get; set; }
    public float CurrentRadius { get; set; }
    public float Opacity { get; set; } // 0-100, how dense the smoke is
    
    // Phases of smoke
    [StringLength(50)]
    public string Phase { get; set; } = null!; // expanding, full, dissipating
    
    public float ExpansionTime { get; set; } // Time to reach full size
    public float FullTime { get; set; } // Time at full opacity
    public float DissipationTime { get; set; } // Time to fully dissipate
    
    // Impact on visibility
    public int PlayersObscured { get; set; } // Players currently inside smoke
    public int SightLinesBlocked { get; set; } // Number of player sight lines blocked
    
    // Tactical usage
    public bool BlocksBombsiteView { get; set; }
    public bool BlocksChoke { get; set; }
    public bool EnabledPlantDefuse { get; set; }
    
    [StringLength(100)]
    public string? TacticalPurpose { get; set; } // execute, retake, isolation, etc.
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    // Environmental factors
    public bool AffectedByWind { get; set; }
    public float WindDirection { get; set; }
    public float WindStrength { get; set; }
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON for additional smoke data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}