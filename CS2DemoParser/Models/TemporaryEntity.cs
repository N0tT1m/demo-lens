using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("TemporaryEntities")]
public class TemporaryEntity
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
    
    public int? PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public virtual Player? Player { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = null!; // muzzle_flash, impact, explosion, beam, decal, etc.
    
    [StringLength(50)]
    public string? SubType { get; set; } // weapon_type for muzzle flash, impact_material, explosion_type, etc.
    
    // Position data
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    // Direction/Angle data (for effects like muzzle flash)
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DirectionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DirectionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DirectionZ { get; set; }
    
    // Effect-specific properties
    public float? Intensity { get; set; } // Effect intensity (0-1)
    public float? Duration { get; set; } // Effect duration in seconds
    public float? Scale { get; set; } // Effect scale/size
    
    [StringLength(50)]
    public string? Material { get; set; } // Surface material for impacts/decals
    
    [StringLength(50)]
    public string? WeaponName { get; set; } // Associated weapon for muzzle flashes
    
    // Beam-specific properties
    [Column(TypeName = "decimal(18,6)")]
    public decimal? EndPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? EndPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? EndPositionZ { get; set; }
    
    public int? TargetEntityId { get; set; } // For beam effects targeting entities
    
    // Visual properties
    [StringLength(20)]
    public string? Color { get; set; } // RGB color for effects
    public float? Alpha { get; set; } // Transparency (0-1)
    
    // Impact-specific data
    public float? ImpactForce { get; set; }
    public bool IsWallbang { get; set; }
    public int? PenetrationCount { get; set; }
    
    // Explosion-specific data
    public float? ExplosionRadius { get; set; }
    public float? DamageRadius { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? AdditionalData { get; set; } // JSON for effect-specific data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}