using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("VoiceCommunications")]
public class VoiceCommunication
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
    public int SpeakerId { get; set; }
    [ForeignKey("SpeakerId")]
    public virtual Player Speaker { get; set; } = null!;
    
    public int StartTick { get; set; }
    public int EndTick { get; set; }
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public float Duration { get; set; }
    
    [Required]
    [StringLength(50)]
    public string CommunicationType { get; set; } = null!; // voice, radio, text
    
    [StringLength(100)]
    public string? RadioCommand { get; set; } // Specific radio command if applicable
    
    [StringLength(100)]
    public string? CommandCategory { get; set; } // tactical, informational, emotional, strategic
    
    [StringLength(100)]
    public string? CommandPurpose { get; set; } // callout, order, information, coordination
    
    // Voice activity analysis
    public float VoiceIntensity { get; set; } // 0-100, how loud/emphatic
    public bool IsUrgent { get; set; }
    public bool IsCalm { get; set; }
    public bool IsEmotional { get; set; }
    
    // Timing and context analysis
    public bool DuringAction { get; set; } // If communication happened during combat
    public bool PreRound { get; set; } // During freeze time
    public bool MidRound { get; set; } // During active round
    public bool PostRound { get; set; } // After round end
    
    [StringLength(100)]
    public string? SituationalContext { get; set; } // clutch, retake, execute, save, etc.
    
    // Target analysis
    public bool ToTeam { get; set; } // Team communication
    public bool ToSpecific { get; set; } // Directed at specific player
    public int? TargetPlayerId { get; set; }
    [ForeignKey("TargetPlayerId")]
    public virtual Player? TargetPlayer { get; set; }
    
    // Communication effectiveness
    public bool WasFollowed { get; set; } // If team acted on the communication
    public bool WasCorrect { get; set; } // If the information was accurate
    public float EffectivenessScore { get; set; } // 0-100, overall effectiveness
    public float ClarityScore { get; set; } // 0-100, how clear the communication was
    
    // Positional context
    [Column(TypeName = "decimal(18,6)")]
    public decimal SpeakerPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal SpeakerPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal SpeakerPositionZ { get; set; }
    
    [StringLength(100)]
    public string? SpeakerArea { get; set; } // Map area where speaker was located
    
    // Content analysis (if text is available)
    [StringLength(255)]
    public string? TranscribedContent { get; set; } // Transcribed voice content
    [StringLength(255)]
    public string? ContentSummary { get; set; } // Summary of communication content
    
    // Team coordination impact
    public bool TriggeredRotation { get; set; }
    public bool TriggeredRegroup { get; set; }
    public bool TriggeredExecute { get; set; }
    public bool TriggeredSave { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    // Leadership and social dynamics
    public bool IsLeadershipCommunication { get; set; }
    public bool IsQuestion { get; set; }
    public bool IsOrder { get; set; }
    public bool IsResponse { get; set; }
    public bool IsCallout { get; set; }
    
    // Interruption analysis
    public bool InterruptedOther { get; set; }
    public bool WasInterrupted { get; set; }
    public int? InterruptedCommunicationId { get; set; }
    
    [StringLength(255)]
    public string? AdditionalData { get; set; } // JSON for additional analysis
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}