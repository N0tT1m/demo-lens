using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerMatchStats")]
public class PlayerMatchStats
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    public int Kills { get; set; }
    
    public int Deaths { get; set; }
    
    public int Assists { get; set; }
    
    public int Score { get; set; }
    
    public int MVPs { get; set; }
    
    public int HeadshotKills { get; set; }
    
    public float HeadshotPercentage { get; set; }
    
    public int TotalDamageDealt { get; set; }
    
    public int TotalDamageReceived { get; set; }
    
    public float ADR { get; set; }
    
    public int FirstKills { get; set; }
    
    public int FirstDeaths { get; set; }
    
    public int TradeKills { get; set; }
    
    public int TradeDeaths { get; set; }
    
    public int SurvivalTime { get; set; }
    
    public float KDRatio { get; set; }
    
    public float Rating { get; set; }
    
    public float HLTV2Rating { get; set; }
    
    public int RoundsPlayed { get; set; }
    
    public int RoundsWon { get; set; }
    
    public int ClutchWins1v1 { get; set; }
    
    public int ClutchWins1v2 { get; set; }
    
    public int ClutchWins1v3 { get; set; }
    
    public int ClutchWins1v4 { get; set; }
    
    public int ClutchWins1v5 { get; set; }
    
    public int ClutchAttempts1v1 { get; set; }
    
    public int ClutchAttempts1v2 { get; set; }
    
    public int ClutchAttempts1v3 { get; set; }
    
    public int ClutchAttempts1v4 { get; set; }
    
    public int ClutchAttempts1v5 { get; set; }
    
    public int FlashAssists { get; set; }
    
    public int UtilityDamage { get; set; }
    
    public int EnemiesFlashed { get; set; }
    
    public int TeammatesFlashed { get; set; }
    
    public float FlashDuration { get; set; }
    
    public int BombPlants { get; set; }
    
    public int BombDefuses { get; set; }
    
    public int HostageRescues { get; set; }
    
    public int MoneySpent { get; set; }
    
    public int MoneyEarned { get; set; }
    
    public int ShotsHit { get; set; }
    
    public int ShotsFired { get; set; }
    
    public float Accuracy { get; set; }
    
    public int WallbangKills { get; set; }
    
    public int CollateralKills { get; set; }
    
    public int NoScopeKills { get; set; }
    
    public int BlindKills { get; set; }
    
    public int SmokeKills { get; set; }
    
    public float KASTPercentage { get; set; }
    
    public int MultiKillRounds2K { get; set; }
    
    public int MultiKillRounds3K { get; set; }
    
    public int MultiKillRounds4K { get; set; }
    
    public int MultiKillRounds5K { get; set; }
}