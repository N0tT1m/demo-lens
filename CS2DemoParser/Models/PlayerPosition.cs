using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerPositions")]
public class PlayerPosition
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VelocityX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VelocityY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VelocityZ { get; set; }
    
    public float Speed { get; set; }
    
    public bool IsAlive { get; set; }
    
    public int Health { get; set; }
    
    public int Armor { get; set; }
    
    public bool HasHelmet { get; set; }
    
    public bool HasDefuseKit { get; set; }
    
    public bool IsScoped { get; set; }
    
    public bool IsWalking { get; set; }
    
    public bool IsCrouching { get; set; }
    
    public bool IsReloading { get; set; }
    
    public bool IsDefusing { get; set; }
    
    public bool IsPlanting { get; set; }
    
    [MaxLength(100)]
    public string? ActiveWeapon { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public int Money { get; set; }
    
    public int FlashDuration { get; set; }
    
    public bool IsBlind { get; set; }
    
    public bool InSmoke { get; set; }
    
    public bool OnLadder { get; set; }
    
    public bool InAir { get; set; }
    
    public bool IsDucking { get; set; }
    
    public float StaminaPercentage { get; set; }
    
    public float LookDistance { get; set; }
}