using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("CommunicationPatterns")]
public class CommunicationPattern
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int DemoFileId { get; set; }
    [ForeignKey("DemoFileId")]
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    public int? RoundId { get; set; }
    [ForeignKey("RoundId")]
    public virtual Round? Round { get; set; }
    
    [Required]
    [StringLength(50)]
    public string PatternType { get; set; } = null!; // leadership, coordination, callout_chain, etc.
    
    [Required]
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int StartTick { get; set; }
    public int EndTick { get; set; }
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public float Duration { get; set; }
    
    public int RoundNumber { get; set; }
    
    // Pattern participants
    public int ParticipantCount { get; set; }
    [StringLength(255)]
    public string ParticipantIds { get; set; } = null!; // JSON array of player IDs
    
    public int? PrimaryLeaderId { get; set; }
    [ForeignKey("PrimaryLeaderId")]
    public virtual Player? PrimaryLeader { get; set; }
    
    // Communication flow analysis
    public float CommunicationDensity { get; set; } // Communications per second
    public float ResponseRate { get; set; } // Percentage of communications that got responses
    public float OverlapPercentage { get; set; } // Percentage of time with overlapping communications
    
    // Pattern quality metrics
    public float CoordinationQuality { get; set; } // 0-100, how well coordinated
    public float InformationQuality { get; set; } // 0-100, quality of information shared
    public float LeadershipClarity { get; set; } // 0-100, clarity of leadership/direction
    public float TeamResponsiveness { get; set; } // 0-100, how responsive team was
    
    // Specific pattern types
    public bool IsExecutePattern { get; set; } // Coordinated execute communication
    public bool IsRetakePattern { get; set; } // Retake coordination
    public bool IsRotationPattern { get; set; } // Rotation coordination
    public bool IsInformationChain { get; set; } // Information sharing chain
    public bool IsLeadershipSequence { get; set; } // Clear leadership sequence
    
    // Tactical outcomes
    public bool AchievedObjective { get; set; }
    public bool ImprovedCoordination { get; set; }
    public bool CausedConfusion { get; set; }
    public bool WastedTime { get; set; }
    
    // Pattern effectiveness
    public float EffectivenessScore { get; set; } // 0-100, overall pattern effectiveness
    public float ImpactOnRound { get; set; } // -100 to +100, impact on round outcome
    
    // Communication content analysis
    [StringLength(100)]
    public string? PrimaryTopic { get; set; } // Main topic of communication
    [StringLength(100)]
    public string? SecondaryTopic { get; set; } // Secondary topic
    
    public int CalloutCount { get; set; }
    public int OrderCount { get; set; }
    public int QuestionCount { get; set; }
    public int ResponseCount { get; set; }
    public int ConfirmationCount { get; set; }
    
    // Timing analysis
    [StringLength(100)]
    public string? OptimalTiming { get; set; } // Whether timing was optimal
    public float TimingScore { get; set; } // 0-100, quality of timing
    
    // Innovation and adaptation
    public bool IsInnovativePattern { get; set; }
    public bool IsAdaptiveResponse { get; set; }
    public bool IsStandardProtocol { get; set; }
    
    [StringLength(255)]
    public string? PatternDescription { get; set; }
    
    [StringLength(255)]
    public string? AdditionalAnalysis { get; set; } // JSON for detailed analysis
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}