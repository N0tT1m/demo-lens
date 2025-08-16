using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerRoundStats")]
public class PlayerRoundStats
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    [ForeignKey(nameof(Round))]
    public int RoundId { get; set; }
    public virtual Round Round { get; set; } = null!;
    
    public int Kills { get; set; }
    
    public int Deaths { get; set; }
    
    public int Assists { get; set; }
    
    public int Damage { get; set; }
    
    public int UtilityDamage { get; set; }
    
    public int StartMoney { get; set; }
    
    public int MoneySpent { get; set; }
    
    public int EndMoney { get; set; }
    
    public int EquipmentValue { get; set; }
    
    public bool IsAlive { get; set; }
    
    public int Health { get; set; }
    
    public int Armor { get; set; }
    
    public bool HasHelmet { get; set; }
    
    public bool HasDefuseKit { get; set; }
    
    public bool HasBomb { get; set; }
    
    public float Rating { get; set; }
    
    public int ShotsFired { get; set; }
    
    public int ShotsHit { get; set; }
    
    public float Accuracy { get; set; }
    
    public bool KAST { get; set; }
    
    public bool MVP { get; set; }
    
    public int FlashAssists { get; set; }
    
    public int EnemiesFlashed { get; set; }
    
    public int TeammatesFlashed { get; set; }
    
    public float FlashDuration { get; set; }
    
    public float SurvivalTime { get; set; }
    
    public int ObjectiveTime { get; set; }
    
    public bool IsClutch { get; set; }
    
    public int ClutchSize { get; set; }
    
    public bool ClutchWon { get; set; }
    
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
}