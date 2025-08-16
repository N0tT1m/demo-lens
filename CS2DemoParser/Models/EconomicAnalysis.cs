using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EconomicAnalyses")]
public class EconomicAnalysis
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    [MaxLength(50)]
    public string Team { get; set; } = string.Empty;
    
    public int Round { get; set; }
    
    public int StartMoney { get; set; }
    
    public int MoneySpent { get; set; }
    
    public int EndMoney { get; set; }
    
    public int EquipmentValue { get; set; }
    
    [MaxLength(50)]
    public string RoundType { get; set; } = string.Empty; // eco, force, full
    
    public bool WonRound { get; set; }
    
    public int MoneyReward { get; set; }
    
    public int KillReward { get; set; }
    
    public int ObjectiveReward { get; set; }
    
    public double EfficiencyRatio { get; set; } // damage per dollar
    
    public int WeaponsDropped { get; set; }
    
    public int WeaponsPicked { get; set; }
    
    public int UtilityBought { get; set; }
    
    public int ArmorBought { get; set; }
    
    public int DefuseKitsBought { get; set; }
    
    public int PlayersWithArmor { get; set; }
    
    public int PlayersWithHelmet { get; set; }
    
    public int RifleCount { get; set; }
    
    public int SMGCount { get; set; }
    
    public int PistolCount { get; set; }
    
    public int SniperCount { get; set; }
    
    public int GrenadeCount { get; set; }
    
    public int FlashCount { get; set; }
    
    public int SmokeCount { get; set; }
    
    public int HECount { get; set; }
    
    public int MolotovCount { get; set; }
    
    public double TeamEconomicHealth { get; set; }
}