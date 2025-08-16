using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerBehaviorEvents")]
public class PlayerBehaviorEvent
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
    
    [Required]
    public int PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public virtual Player Player { get; set; } = null!;
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string BehaviorType { get; set; } = null!; // footstep, jump, sound, inspect_weapon, reload, etc.
    
    [StringLength(50)]
    public string? BehaviorSubType { get; set; } // walk, run, land_hard, land_soft, etc.
    
    // Position data
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    // Movement data
    [Column(TypeName = "decimal(18,6)")]
    public decimal? VelocityX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? VelocityY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? VelocityZ { get; set; }
    
    public float? Speed { get; set; }
    public float? Direction { get; set; } // Movement direction in degrees
    
    // View angles
    [Column(TypeName = "decimal(18,6)")]
    public decimal? ViewAngleX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? ViewAngleY { get; set; }
    
    // Sound-related data
    public float? SoundVolume { get; set; } // Volume of sound made (0-1)
    public float? SoundRadius { get; set; } // How far sound travels
    
    [StringLength(50)]
    public string? SoundType { get; set; } // footstep, weapon, voice, etc.
    
    [StringLength(50)]
    public string? SurfaceMaterial { get; set; } // Surface type for footsteps
    
    // Weapon-related behavior
    [StringLength(50)]
    public string? WeaponName { get; set; } // Weapon being used/inspected
    
    public bool IsWeaponInspection { get; set; }
    public bool IsWeaponReload { get; set; }
    public bool IsWeaponDraw { get; set; }
    public bool IsWeaponHolster { get; set; }
    
    // Jump/Fall data
    public float? JumpHeight { get; set; }
    public float? FallDistance { get; set; }
    public float? FallDamage { get; set; }
    public float? LandingImpact { get; set; } // Force of landing
    
    // Movement states
    public bool IsWalking { get; set; }
    public bool IsRunning { get; set; }
    public bool IsCrouching { get; set; }
    public bool IsInAir { get; set; }
    public bool IsOnLadder { get; set; }
    public bool IsInWater { get; set; }
    
    // Stealth analysis
    public bool IsSilentMovement { get; set; } // Walking to reduce noise
    public bool IsAudibleToEnemies { get; set; } // Can enemies hear this?
    
    public float? StealthScore { get; set; } // How stealthy the movement was (0-100)
    
    // Tactical context
    [StringLength(50)]
    public string? TacticalContext { get; set; } // peek, rotate, hold_angle, rush, etc.
    
    public bool IsPeeking { get; set; }
    public bool IsRetreating { get; set; }
    public bool IsAdvancing { get; set; }
    public bool IsHoldingAngle { get; set; }
    
    // Timing analysis
    public float? TimeSinceLastAction { get; set; } // Time since last significant action
    public float? ActionDuration { get; set; } // Duration of this behavior
    
    // Impact analysis
    public bool WasCompromising { get; set; } // Did this behavior give away position?
    public bool WasTactical { get; set; } // Was this a good tactical decision?
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}