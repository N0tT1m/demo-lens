using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("RoundImpacts")]
public class RoundImpact
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
    
    [Required]
    public int RoundId { get; set; }
    [ForeignKey("RoundId")]
    public virtual Round Round { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    // Overall impact metrics
    public float OverallImpact { get; set; } // -100 to +100, overall round impact
    public float PositiveImpact { get; set; } // 0-100, positive contributions
    public float NegativeImpact { get; set; } // 0-100, negative impact (mistakes)
    public float NetImpact { get; set; } // Positive - negative
    
    // Specific impact categories
    public float FraggingImpact { get; set; } // Impact from kills/damage
    public float UtilityImpact { get; set; } // Impact from utility usage
    public float PositionalImpact { get; set; } // Impact from positioning/map control
    public float EconomicImpact { get; set; } // Impact on team economy
    public float TacticalImpact { get; set; } // Impact on team tactics/strategy
    
    // Key moments
    public bool HasEntryFrag { get; set; }
    public bool HasClutchAttempt { get; set; }
    public bool HasClutchWin { get; set; }
    public bool HasMultiKill { get; set; }
    public bool HasCriticalSave { get; set; }
    public bool HasGameChangingPlay { get; set; }
    
    // Timing impact
    public float EarlyRoundImpact { get; set; } // Impact in first 30 seconds
    public float MidRoundImpact { get; set; } // Impact in middle phase
    public float LateRoundImpact { get; set; } // Impact in final phase
    
    // Situational performance
    public float WinRoundContribution { get; set; } // Contribution to round win
    public float LossRoundImpact { get; set; } // How much they minimized loss
    public float RoundOutcomePrediction { get; set; } // How predictive their performance was
    
    // Decision quality
    public float DecisionQuality { get; set; } // Quality of key decisions
    public int GoodDecisions { get; set; }
    public int BadDecisions { get; set; }
    public int CriticalDecisions { get; set; }
    
    // Team impact
    public float TeamSupportImpact { get; set; } // How much they helped teammates
    public float LeadershipImpact { get; set; } // Leadership/calling impact
    public float FollowupImpact { get; set; } // Following team plans/calls
    
    // Risk/reward analysis
    public float RiskTaken { get; set; } // Amount of risk in their plays
    public float RewardAchieved { get; set; } // Reward from risk-taking
    public float RiskRewardRatio { get; set; } // Efficiency of risk-taking
    
    // Momentum impact
    public float MomentumGenerated { get; set; } // Positive momentum for team
    public float MomentumLost { get; set; } // Momentum lost due to mistakes
    public float MomentumShift { get; set; } // Net momentum change
    
    // Context-specific impact
    [StringLength(50)]
    public string RoundType { get; set; } = null!; // eco, force, full_buy, etc.
    public float RoundTypeImpact { get; set; } // Impact relative to round type expectations
    
    [StringLength(100)]
    public string? KeyMoment { get; set; } // Most important moment of the round
    public float KeyMomentImpact { get; set; } // Impact of the key moment
    
    // Advanced metrics
    public float WinProbabilityContribution { get; set; } // How much they changed win probability
    public float ExpectedValue { get; set; } // Expected performance based on situation
    public float PerformanceVsExpected { get; set; } // Actual vs expected performance
    
    [StringLength(255)]
    public string? ImpactSummary { get; set; } // Human-readable impact summary
    
    [StringLength(255)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}