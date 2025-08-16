using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Grenades")]
public class Grenade
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Round))]
    public int RoundId { get; set; }
    public virtual Round Round { get; set; } = null!;
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    public int ThrowTick { get; set; }
    
    public float ThrowTime { get; set; }
    
    public int? DetonateTick { get; set; }
    
    public float? DetonateTime { get; set; }
    
    [MaxLength(50)]
    public string? GrenadeType { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowPositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowPositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DetonatePositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DetonatePositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DetonatePositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowVelocityX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowVelocityY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowVelocityZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ThrowAngleY { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public float FlightTime { get; set; }
    
    public float EffectRadius { get; set; }
    
    public int PlayersAffected { get; set; }
    
    public int EnemiesAffected { get; set; }
    
    public int TeammatesAffected { get; set; }
    
    public float TotalDamage { get; set; }
    
    public float TotalFlashDuration { get; set; }
    
    public bool IsLineup { get; set; }
    
    public bool IsBounce { get; set; }
    
    public int BounceCount { get; set; }
    
    public bool IsRunThrow { get; set; }
    
    public bool IsJumpThrow { get; set; }
    
    public bool IsStandingThrow { get; set; }
    
    public bool IsCrouchThrow { get; set; }
    
    [MaxLength(100)]
    public string? ThrowStyle { get; set; }
    
    public float ThrowStrength { get; set; }
}