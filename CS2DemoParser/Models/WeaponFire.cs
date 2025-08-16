using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("WeaponFires")]
public class WeaponFire
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Round))]
    public int RoundId { get; set; }
    public virtual Round Round { get; set; } = null!;
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    [MaxLength(100)]
    public string? Weapon { get; set; }
    
    [MaxLength(50)]
    public string? WeaponClass { get; set; }
    
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
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public bool IsScoped { get; set; }
    
    public bool IsSilenced { get; set; }
    
    public int Ammo { get; set; }
    
    public int AmmoReserve { get; set; }
    
    public float RecoilIndex { get; set; }
    
    public float Accuracy { get; set; }
    
    public float Velocity { get; set; }
    
    public bool ThroughSmoke { get; set; }
    
    public bool IsBlind { get; set; }
    
    public int FlashDuration { get; set; }
}