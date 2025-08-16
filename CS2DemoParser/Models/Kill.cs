using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Kills")]
public class Kill
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
    
    [ForeignKey(nameof(Killer))]
    public int? KillerId { get; set; }
    public virtual Player? Killer { get; set; }
    
    [ForeignKey(nameof(Victim))]
    public int VictimId { get; set; }
    public virtual Player Victim { get; set; } = null!;
    
    [ForeignKey(nameof(Assister))]
    public int? AssisterId { get; set; }
    public virtual Player? Assister { get; set; }
    
    [MaxLength(100)]
    public string? Weapon { get; set; }
    
    [MaxLength(50)]
    public string? WeaponClass { get; set; }
    
    [MaxLength(50)]
    public string? HitGroup { get; set; }
    
    public bool IsHeadshot { get; set; }
    
    public bool IsWallbang { get; set; }
    
    public bool IsNoScope { get; set; }
    
    public bool IsBlind { get; set; }
    
    public bool IsSmoke { get; set; }
    
    public bool IsFlash { get; set; }
    
    public bool IsCollateral { get; set; }
    
    public bool IsFirstKill { get; set; }
    
    public bool IsTradeKill { get; set; }
    
    public bool IsClutch { get; set; }
    
    public int ClutchSize { get; set; }
    
    public float Distance { get; set; }
    
    public int Damage { get; set; }
    
    public int Penetration { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal KillerPositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal KillerPositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal KillerPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal KillerViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal KillerViewAngleY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimViewAngleY { get; set; }
    
    public int KillerHealth { get; set; }
    
    public int KillerArmor { get; set; }
    
    public int VictimHealth { get; set; }
    
    public int VictimArmor { get; set; }
    
    [MaxLength(100)]
    public string? AssistType { get; set; }
    
    public float AssistDistance { get; set; }
    
    public float TimeSinceLastDamage { get; set; }
    
    public bool IsRevengeKill { get; set; }
    
    public bool IsDominating { get; set; }
    
    public bool IsRevenge { get; set; }
    
    [MaxLength(100)]
    public string? KillerTeam { get; set; }
    
    [MaxLength(100)]
    public string? VictimTeam { get; set; }
    
    public bool IsTeamKill { get; set; }
    
    public int FlashDuration { get; set; }
    
    public bool ThroughSmoke { get; set; }
    
    public bool AttackerBlind { get; set; }
    
    public bool VictimBlind { get; set; }
}