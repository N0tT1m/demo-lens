using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("AdvancedUserMessages")]
public class AdvancedUserMessage
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
    
    public int? PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public virtual Player? Player { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string MessageType { get; set; } = null!; // damage_report, xp_update, vote_start, deep_stats, etc.
    
    [StringLength(50)]
    public string? MessageSubType { get; set; } // vote_type, stat_category, etc.
    
    // Generic message data
    [StringLength(255)]
    public string? MessageContent { get; set; } // Message text/content
    
    [StringLength(500)]
    public string? MessageData { get; set; } // JSON data for complex messages
    
    // Damage report specific
    public int? DamageGiven { get; set; }
    public int? DamageTaken { get; set; }
    public int? HitsGiven { get; set; }
    public int? HitsTaken { get; set; }
    
    public int? TargetPlayerId { get; set; } // For damage reports
    [ForeignKey("TargetPlayerId")]
    public virtual Player? TargetPlayer { get; set; }
    
    // XP/Progress specific
    public int? XpGained { get; set; }
    public int? XpTotal { get; set; }
    public int? QuestProgress { get; set; }
    public int? QuestGoal { get; set; }
    
    [StringLength(100)]
    public string? QuestName { get; set; }
    
    // Vote system specific
    [StringLength(100)]
    public string? VoteTarget { get; set; } // Player being voted on
    [StringLength(50)]
    public string? VoteType { get; set; } // kick, ban, change_map, etc.
    [StringLength(100)]
    public string? VoteReason { get; set; }
    
    public int? VotesRequired { get; set; }
    public int? VotesFor { get; set; }
    public int? VotesAgainst { get; set; }
    public bool? VotePassed { get; set; }
    
    // Stats specific
    [StringLength(50)]
    public string? StatCategory { get; set; } // performance, accuracy, economy, etc.
    
    public float? StatValue { get; set; }
    public float? StatPercentile { get; set; } // Player's percentile ranking
    
    [StringLength(50)]
    public string? StatComparison { get; set; } // vs_team_average, vs_match_average, etc.
    
    // Leaderboard specific
    public int? LeaderboardRank { get; set; }
    public int? LeaderboardScore { get; set; }
    
    [StringLength(50)]
    public string? LeaderboardType { get; set; } // kills, score, mvp, etc.
    
    // Money/Economy specific
    public int? MoneyChange { get; set; }
    public int? MoneyTotal { get; set; }
    
    [StringLength(50)]
    public string? MoneyReason { get; set; } // kill_reward, loss_bonus, plant_reward, etc.
    
    // UI/Display specific
    public float? DisplayDuration { get; set; } // How long message was shown
    
    [StringLength(50)]
    public string? DisplayLocation { get; set; } // hud, center, console, etc.
    
    public bool IsImportant { get; set; } // High priority message
    public bool IsServerMessage { get; set; } // From server vs game
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}