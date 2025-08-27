using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("WeaponStateChanges")]
public class WeaponStateChange
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
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [MaxLength(100)]
    public string WeaponName { get; set; } = "";
    
    [MaxLength(50)]
    public string? WeaponClass { get; set; }
    
    [MaxLength(50)]
    public string EventType { get; set; } = "";  // "Pickup", "Drop", "Switch", "Reload", "Fire", "Zoom"
    
    // Weapon state information from demofile-net
    public int AmmoClip { get; set; }
    public int AmmoReserve { get; set; }
    public bool IsReloading { get; set; }
    public bool IsZoomed { get; set; }
    public float ZoomLevel { get; set; }
    
    // Position and view angle when weapon event occurred
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
    
    // Weapon switch analysis
    [MaxLength(100)]
    public string? PreviousWeapon { get; set; }
    
    public float SwitchTime { get; set; }  // Time taken to switch weapons
    
    // Enhanced tracking
    [MaxLength(100)]
    public string? WeaponItemId { get; set; }  // Unique weapon instance ID
    
    [MaxLength(100)]
    public string? OriginalOwnerSteamId { get; set; }  // For dropped weapons
    
    public bool IsDropped { get; set; }
    public bool IsThrown { get; set; }  // For grenades
    
    // Advanced weapon analytics
    public int ShotsFiredSinceLastEvent { get; set; }
    public float AccuracySinceLastEvent { get; set; }
    public bool WasKillShot { get; set; }
}