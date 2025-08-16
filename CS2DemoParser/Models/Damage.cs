using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Damages")]
public class Damage
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
    
    [ForeignKey(nameof(Attacker))]
    public int? AttackerId { get; set; }
    public virtual Player? Attacker { get; set; }
    
    [ForeignKey(nameof(Victim))]
    public int VictimId { get; set; }
    public virtual Player Victim { get; set; } = null!;
    
    [MaxLength(100)]
    public string? Weapon { get; set; }
    
    [MaxLength(50)]
    public string? WeaponClass { get; set; }
    
    [MaxLength(50)]
    public string? HitGroup { get; set; }
    
    public int DamageAmount { get; set; }
    
    public int DamageArmor { get; set; }
    
    public int Health { get; set; }
    
    public int Armor { get; set; }
    
    public bool IsHeadshot { get; set; }
    
    public bool IsWallbang { get; set; }
    
    public bool IsFatal { get; set; }
    
    public float Distance { get; set; }
    
    public int Penetration { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AttackerPositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AttackerPositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AttackerPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AttackerViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AttackerViewAngleY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VictimViewAngleY { get; set; }
    
    [MaxLength(100)]
    public string? AttackerTeam { get; set; }
    
    [MaxLength(100)]
    public string? VictimTeam { get; set; }
    
    public bool IsTeamDamage { get; set; }
    
    public bool ThroughSmoke { get; set; }
    
    public bool AttackerBlind { get; set; }
    
    public bool VictimBlind { get; set; }
    
    public int FlashDuration { get; set; }
    
    public bool IsNoScope { get; set; }
}