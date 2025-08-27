using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EnhancedPlayerPositions")]
public class EnhancedPlayerPosition
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
    
    // Position data from demofile-net
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionZ { get; set; }
    
    // View angles for crosshair placement analysis
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleY { get; set; }
    
    // Velocity data
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float VelocityZ { get; set; }
    public float Speed { get; set; }  // Total velocity magnitude
    
    // Player state from demofile-net
    public int Health { get; set; }
    public int Armor { get; set; }
    public bool HasHelmet { get; set; }
    public bool IsAlive { get; set; }
    public bool IsDefusing { get; set; }
    public bool IsPlanting { get; set; }
    public bool IsReloading { get; set; }
    public bool IsScoped { get; set; }
    public bool IsWalking { get; set; }
    public bool IsDucking { get; set; }
    public bool IsBlinded { get; set; }
    
    // Current weapon information
    [MaxLength(100)]
    public string? ActiveWeapon { get; set; }
    
    [MaxLength(50)]
    public string? ActiveWeaponClass { get; set; }
    
    public int AmmoClip { get; set; }
    public int AmmoReserve { get; set; }
    
    // Money and equipment value
    public int Money { get; set; }
    public int EquipmentValue { get; set; }
    
    // Advanced positional analysis
    [MaxLength(50)]
    public string? MapArea { get; set; }  // "A Site", "B Site", "Mid", etc.
    
    [MaxLength(50)]
    public string? PositionType { get; set; }  // "Holding", "Rotating", "Peeking", "Hiding"
    
    public float DistanceToNearestEnemy { get; set; }
    public float DistanceToNearestTeammate { get; set; }
    
    // Line of sight information
    public int VisibleEnemies { get; set; }
    public int VisibleTeammates { get; set; }
    
    // Tactical state
    public bool IsInSmokeArea { get; set; }
    public bool IsInFlashArea { get; set; }
    public bool IsInFireArea { get; set; }
    public bool HasLineOfSightToBomb { get; set; }
    
    // Movement pattern analysis
    public float MovementAcceleration { get; set; }
    public float ViewAngleChangeRate { get; set; }  // How fast they're turning
    public bool IsCounterStrafing { get; set; }
    public bool IsPeeking { get; set; }
    
    // Team coordination
    public bool IsWithTeammates { get; set; }  // Within 500 units of teammate
    public int TeammatesNearby { get; set; }  // Count of teammates within 1000 units
}