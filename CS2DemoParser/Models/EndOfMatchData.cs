using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EndOfMatchData")]
public class EndOfMatchData
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    public DateTime MatchEndTime { get; set; }
    
    public float MatchDurationSeconds { get; set; }
    
    [MaxLength(50)]
    public string WinningTeam { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string WinReason { get; set; } = string.Empty;
    
    public int FinalScoreCT { get; set; }
    
    public int FinalScoreT { get; set; }
    
    public bool WasOvertime { get; set; }
    
    public int OvertimeRounds { get; set; }
    
    [MaxLength(100)]
    public string? MVP { get; set; }
    
    public double MVPRating { get; set; }
    
    public int TotalKills { get; set; }
    
    public int TotalDeaths { get; set; }
    
    public int TotalAssists { get; set; }
    
    public int TotalDamage { get; set; }
    
    public double AverageRating { get; set; }
    
    [MaxLength(1000)]
    public string? AdditionalData { get; set; }
}