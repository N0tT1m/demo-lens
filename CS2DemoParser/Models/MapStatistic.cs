using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("MapStatistics")]
public class MapStatistic
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    [MaxLength(100)]
    public string MapName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Team { get; set; } = string.Empty;
    
    public double BombsiteAControlPercentage { get; set; }
    
    public double BombsiteBControlPercentage { get; set; }
    
    public double MidControlPercentage { get; set; }
    
    public int BombsiteAPlants { get; set; }
    
    public int BombsiteBPlants { get; set; }
    
    public int BombsiteADefuses { get; set; }
    
    public int BombsiteBDefuses { get; set; }
    
    public int BombsiteAExplosions { get; set; }
    
    public int BombsiteBExplosions { get; set; }
    
    public double AverageRoundTime { get; set; }
    
    public int FastestRoundTime { get; set; }
    
    public int SlowestRoundTime { get; set; }
    
    public int TotalSmokeGrenades { get; set; }
    
    public int TotalFlashbangs { get; set; }
    
    public int TotalHEGrenades { get; set; }
    
    public int TotalMolotovs { get; set; }
    
    public double UtilityEffectiveness { get; set; }
    
    public int WallbangKills { get; set; }
    
    public int LongRangeKills { get; set; }
    
    public int CloseRangeKills { get; set; }
    
    public double AverageKillDistance { get; set; }
    
    public int BoostKills { get; set; }
    
    public int NoScopeKills { get; set; }
    
    public int ThroughSmokeKills { get; set; }
    
    public int BlindKills { get; set; }
    
    public double MovementAccuracy { get; set; }
    
    public double StationaryAccuracy { get; set; }
    
    public double CrouchingAccuracy { get; set; }
    
    public double JumpingAccuracy { get; set; }
}