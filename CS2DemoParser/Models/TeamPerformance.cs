using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("TeamPerformances")]
public class TeamPerformance
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    [MaxLength(50)]
    public string TeamName { get; set; } = string.Empty;
    
    public int RoundsWon { get; set; }
    
    public int RoundsLost { get; set; }
    
    public int TotalKills { get; set; }
    
    public int TotalDeaths { get; set; }
    
    public int TotalAssists { get; set; }
    
    public int TotalDamage { get; set; }
    
    public double AverageRating { get; set; }
    
    public int EcoRoundsWon { get; set; }
    
    public int EcoRoundsPlayed { get; set; }
    
    public int ForceRoundsWon { get; set; }
    
    public int ForceRoundsPlayed { get; set; }
    
    public int FullBuyRoundsWon { get; set; }
    
    public int FullBuyRoundsPlayed { get; set; }
    
    public int PistolRoundsWon { get; set; }
    
    public int PistolRoundsPlayed { get; set; }
    
    public double FirstKillPercentage { get; set; }
    
    public double ClutchSuccessRate { get; set; }
    
    public int TotalClutches { get; set; }
    
    public int SuccessfulClutches { get; set; }
    
    public double AverageADR { get; set; }
    
    public double KAST { get; set; }
    
    public int MultiKillRounds { get; set; }
    
    public int TotalFlashAssists { get; set; }
    
    public double UtilityDamage { get; set; }
}