using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PerformanceMetrics")]
public class PerformanceMetric
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DemoFileId { get; set; }
    [ForeignKey("DemoFileId")]
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [Required]
    public int PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public virtual Player Player { get; set; } = null!;
    
    public int? RoundId { get; set; }
    [ForeignKey("RoundId")]
    public virtual Round? Round { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(100)]
    public string MetricType { get; set; } = null!; // aim_accuracy, movement_efficiency, positioning_quality, etc.
    
    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = null!; // Specific metric identifier
    
    public float Value { get; set; } // The metric value
    public float NormalizedValue { get; set; } // Value normalized to 0-100 scale
    public float Confidence { get; set; } // Confidence in the measurement (0-100)
    
    [StringLength(50)]
    public string? Context { get; set; } // Round context (eco, force, full_buy, etc.)
    
    [StringLength(100)]
    public string? Situation { get; set; } // Specific situation (clutch, entry, trade, etc.)
    
    public int RoundNumber { get; set; }
    
    // Comparative metrics
    public float TeamAverage { get; set; } // Team average for this metric
    public float MatchAverage { get; set; } // Match average for this metric
    public float PercentileRank { get; set; } // Percentile compared to similar situations
    
    // Trend analysis
    public float MovingAverage { get; set; } // Moving average over recent rounds
    public float Trend { get; set; } // Positive = improving, negative = declining
    public bool IsImproving { get; set; }
    public bool IsDecreasing { get; set; }
    
    // Impact assessment
    public float ImpactScore { get; set; } // How much this metric affected round outcome
    public bool PositiveImpact { get; set; }
    public bool NegativeImpact { get; set; }
    
    [StringLength(255)]
    public string? AdditionalData { get; set; } // JSON for extra metric-specific data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}