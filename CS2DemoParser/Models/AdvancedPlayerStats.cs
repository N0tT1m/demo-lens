using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("AdvancedPlayerStats")]
public class AdvancedPlayerStats
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
    
    public int? MatchId { get; set; }
    [ForeignKey("MatchId")]
    public virtual Match? Match { get; set; }
    
    public int? RoundId { get; set; }
    [ForeignKey("RoundId")]
    public virtual Round? Round { get; set; }
    
    public int RoundNumber { get; set; }
    
    [Required]
    [StringLength(50)]
    public string StatsType { get; set; } = null!; // round, match, overall
    
    // Core performance metrics
    public float HLTVRating { get; set; } // HLTV 2.0 rating
    public float HLTVRating1 { get; set; } // Original HLTV rating
    public float ImpactRating { get; set; } // Impact-adjusted rating
    public float KASTPercentage { get; set; } // Kill, Assist, Survive, Trade percentage
    
    // Kill metrics
    public float KillsPerRound { get; set; }
    public float DeathsPerRound { get; set; }
    public float AssistsPerRound { get; set; }
    public float KillDeathRatio { get; set; }
    public float KillAssistSurviveTradeRatio { get; set; }
    
    // Damage and impact
    public float AverageDamagePerRound { get; set; }
    public float DamagePerRound { get; set; }
    public float EffectiveDamagePerRound { get; set; } // Damage that leads to kills
    public float WastedDamage { get; set; } // Damage over 100 per kill
    public float DamageEfficiency { get; set; } // Effective damage / total damage
    
    // First engagement metrics
    public float FirstKillsPerRound { get; set; }
    public float FirstDeathsPerRound { get; set; }
    public float FirstKillRatio { get; set; } // First kills / first deaths
    public float OpeningDuelSuccessRate { get; set; } // Win rate in 1v1 opening duels
    
    // Clutch performance
    public int Clutch1v1Attempts { get; set; }
    public int Clutch1v1Wins { get; set; }
    public int Clutch1v2Attempts { get; set; }
    public int Clutch1v2Wins { get; set; }
    public int Clutch1v3Attempts { get; set; }
    public int Clutch1v3Wins { get; set; }
    public int Clutch1v4Attempts { get; set; }
    public int Clutch1v4Wins { get; set; }
    public int Clutch1v5Attempts { get; set; }
    public int Clutch1v5Wins { get; set; }
    public float OverallClutchSuccessRate { get; set; }
    
    // Multi-kill events
    public int DoubleKills { get; set; }
    public int TripleKills { get; set; }
    public int QuadKills { get; set; }
    public int PentaKills { get; set; }
    public float MultiKillsPerRound { get; set; }
    
    // Survival and positioning
    public float SurvivalRate { get; set; } // Percentage of rounds survived
    public float TradeKillPercentage { get; set; } // Percentage of deaths that were traded
    public float TradeFragPercentage { get; set; } // Percentage of kills that were trades
    public float SupportRoundPercentage { get; set; } // Rounds with assists but no kills
    
    // Weapon efficiency
    public float HeadshotPercentage { get; set; }
    public float RifleKillsPercentage { get; set; }
    public float PistolKillsPercentage { get; set; }
    public float SniperKillsPercentage { get; set; }
    public float AwpKillsPerRound { get; set; }
    public float ShotAccuracy { get; set; } // Shots hit / shots fired
    public float KillsPerShot { get; set; } // Kills / shots fired
    
    // Utility usage and effectiveness
    public float UtilityDamagePerRound { get; set; }
    public float FlashAssistsPerRound { get; set; }
    public float UtilitySuccessRate { get; set; } // Effective utility usage rate
    public float EnemiesFlashedPerRound { get; set; }
    public float TeamFlashesPerRound { get; set; } // Negative utility
    
    // Economic impact
    public float EconomicImpact { get; set; } // Value created - value lost
    public float SavedRoundsImpact { get; set; } // Performance in save rounds
    public float ForceRoundsImpact { get; set; } // Performance in force buy rounds
    public float EcoRoundsImpact { get; set; } // Performance in eco rounds
    
    // Timing and decision making
    public float EntryFragPercentage { get; set; } // First kills of the round
    public float LurkKillsPercentage { get; set; } // Solo kills away from team
    public float RotationTimingScore { get; set; } // Quality of rotation timing
    public float DecisionMakingScore { get; set; } // Overall decision quality metric
    
    // Advanced situational metrics
    public float RoundsWithKill { get; set; } // Percentage of rounds with at least one kill
    public float RoundsWithMultiKill { get; set; } // Percentage of rounds with 2+ kills
    public float RoundsWithZeroKills { get; set; } // Percentage of rounds with no kills
    public float HighImpactRounds { get; set; } // Rounds with significant impact
    
    // Team play metrics
    public float TeamPlayScore { get; set; } // Overall team contribution
    public float CommunicationScore { get; set; } // Quality of callouts (if available)
    public float LeadershipScore { get; set; } // Leadership qualities (if detectable)
    
    // Consistency metrics
    public float PerformanceVariance { get; set; } // Consistency across rounds
    public float ClutchConsistency { get; set; } // Consistency in clutch situations
    public float EconomyAdaptability { get; set; } // Performance across economic states
    
    // Calculated at different timeframes
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public int SampleSize { get; set; } // Number of rounds/matches used for calculation
    
    [StringLength(255)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}