using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("TacticalEvents")]
public class TacticalEvent
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
    
    public int? InitiatorPlayerId { get; set; }
    [ForeignKey("InitiatorPlayerId")]
    public virtual Player? InitiatorPlayer { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = null!; // execute, retake, rotate, stack, split, rush, slow, fake
    
    [Required]
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    // Event details
    [StringLength(100)]
    public string? TargetArea { get; set; } // Which area the tactic targets
    [StringLength(100)]
    public string? SecondaryArea { get; set; } // Secondary area involved
    
    public int PlayersInvolved { get; set; }
    public float Coordination { get; set; } // 0-100, how coordinated the execution was
    public float Timing { get; set; } // 0-100, timing quality of the execution
    
    // Utility usage in the tactic
    public int SmokesUsed { get; set; }
    public int FlashesUsed { get; set; }
    public int HEGrenadesUsed { get; set; }
    public int MolotovsUsed { get; set; }
    public int DecoysUsed { get; set; }
    
    // Execution metrics
    public bool WasSuccessful { get; set; }
    public float SuccessRate { get; set; } // 0-100, based on similar situations
    public float ExecutionQuality { get; set; } // 0-100, overall execution quality
    
    // Timing and positioning
    public float StartTime { get; set; }
    public float? EndTime { get; set; }
    public float Duration { get; set; }
    public bool WasRushed { get; set; }
    public bool WasDelayed { get; set; }
    
    // Counter-tactics
    [StringLength(100)]
    public string? CounterTactic { get; set; } // What the enemy team did in response
    public bool WasCountered { get; set; }
    public float CounterEffectiveness { get; set; } // 0-100
    
    // Outcome tracking
    public int KillsGenerated { get; set; }
    public int DeathsCaused { get; set; }
    public float DamageDealt { get; set; }
    public bool AchievedObjective { get; set; } // Did the tactic achieve its goal
    
    // Strategic context
    [StringLength(100)]
    public string? RoundContext { get; set; } // eco, force, anti_eco, etc.
    [StringLength(100)]
    public string? StrategicIntent { get; set; } // area_control, picks, information, etc.
    
    // Innovation and adaptation
    public bool IsInnovativePlay { get; set; }
    public bool IsAdaptation { get; set; } // Response to enemy tactics
    public float Unpredictability { get; set; } // 0-100, how unexpected the tactic was
    
    [StringLength(255)]
    public string? TacticalNotes { get; set; } // Detailed description
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}