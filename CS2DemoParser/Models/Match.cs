using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Matches")]
public class Match
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [MaxLength(100)]
    public string? MapName { get; set; }
    
    [MaxLength(50)]
    public string? GameMode { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public int TotalRounds { get; set; }
    
    public int CTScore { get; set; }
    
    public int TScore { get; set; }
    
    public int CTScoreFirstHalf { get; set; }
    
    public int TScoreFirstHalf { get; set; }
    
    public int CTScoreSecondHalf { get; set; }
    
    public int TScoreSecondHalf { get; set; }
    
    public int? CTScoreOvertime { get; set; }
    
    public int? TScoreOvertime { get; set; }
    
    public bool IsOvertime { get; set; }
    
    public bool IsFinished { get; set; }
    
    [MaxLength(50)]
    public string? WinnerTeam { get; set; }
    
    [MaxLength(100)]
    public string? WinCondition { get; set; }
    
    public int MaxRounds { get; set; }
    
    public float RoundTimeLimit { get; set; }
    
    public float FreezeTime { get; set; }
    
    public float BuyTime { get; set; }
    
    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();
    public virtual ICollection<PlayerMatchStats> PlayerMatchStats { get; set; } = new List<PlayerMatchStats>();
}