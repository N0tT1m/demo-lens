using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EntityEffects")]
public class EntityEffect
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
    
    public int? SourcePlayerId { get; set; }
    [ForeignKey("SourcePlayerId")]
    public virtual Player? SourcePlayer { get; set; }
    
    [Required]
    [StringLength(100)]
    public string SourceEntityType { get; set; } = null!; // grenade, molotov, smoke, flashbang, etc.
    
    [Required]
    [StringLength(100)]
    public string SourceEntityName { get; set; } = null!;
    
    [Required]
    [StringLength(50)]
    public string EffectType { get; set; } = null!; // damage, blind, slow, burn, obscure
    
    public int StartTick { get; set; }
    public int? EndTick { get; set; }
    public float StartTime { get; set; }
    public float? EndTime { get; set; }
    public float Duration { get; set; }
    
    // Effect area center
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal CenterZ { get; set; }
    
    public float Radius { get; set; }
    public float MaxIntensity { get; set; }
    public float CurrentIntensity { get; set; }
    
    // Affected players tracking
    public int PlayersAffected { get; set; }
    public int EnemiesAffected { get; set; }
    public int TeammatesAffected { get; set; }
    
    public float TotalDamageDealt { get; set; }
    public float MaxDamageToSinglePlayer { get; set; }
    
    // Environmental impact
    public bool BlocksVision { get; set; }
    public bool CausesDamage { get; set; }
    public bool ImpairsMovement { get; set; }
    
    [StringLength(50)]
    public string? Team { get; set; }
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON for additional effect data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}