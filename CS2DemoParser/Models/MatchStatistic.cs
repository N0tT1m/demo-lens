using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("MatchStatistics")]
public class MatchStatistic
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    [MaxLength(100)]
    public string StatisticName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    public double Value { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string? Team { get; set; }
    
    [MaxLength(100)]
    public string? PlayerName { get; set; }
    
    public int? Round { get; set; }
}