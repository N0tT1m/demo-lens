using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("GrenadeTrajectories")]
public class GrenadeTrajectory
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
    
    public int ThrowTick { get; set; }
    public float ThrowTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string GrenadeType { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowVelocityX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowVelocityY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowVelocityZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowAngleX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ThrowAngleY { get; set; }
    
    public int? DetonateTick { get; set; }
    public float? DetonateTime { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DetonatePositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DetonatePositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? DetonatePositionZ { get; set; }
    
    public float? FlightTime { get; set; }
    public int? BounceCount { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public bool IsRunThrow { get; set; }
    public bool IsJumpThrow { get; set; }
    public bool IsCrouchThrow { get; set; }
    
    [StringLength(100)]
    public string? ThrowStyle { get; set; }
    
    public int PlayersAffected { get; set; }
    public int EnemiesAffected { get; set; }
    public int TeammatesAffected { get; set; }
    
    public float? EffectRadius { get; set; }
    public float? DamageDealt { get; set; }
    public float? FlashDuration { get; set; }
    
    public bool IsLineup { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}