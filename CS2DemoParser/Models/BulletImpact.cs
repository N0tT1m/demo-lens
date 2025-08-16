using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("BulletImpacts")]
public class BulletImpact
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
    
    [StringLength(100)]
    public string Weapon { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ShooterPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ShooterPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ShooterPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ImpactPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ImpactPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ImpactPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ShootAngleX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ShootAngleY { get; set; }
    
    public float Distance { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public bool IsScoped { get; set; }
    public bool IsMoving { get; set; }
    public bool IsCrouching { get; set; }
    
    public int PenetrationCount { get; set; }
    
    [StringLength(100)]
    public string? SurfaceType { get; set; }
    
    public bool HitPlayer { get; set; }
    public int? HitPlayerId { get; set; }
    [ForeignKey("HitPlayerId")]
    public virtual Player? HitPlayerEntity { get; set; }
    
    [StringLength(50)]
    public string? HitGroup { get; set; }
    
    public float? DamageDealt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}