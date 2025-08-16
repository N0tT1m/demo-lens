using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("RoundOutcomes")]
public class RoundOutcome
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    [MaxLength(50)]
    public string WinningTeam { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string WinReason { get; set; } = string.Empty;
    
    public float RoundTime { get; set; }
    
    public bool IsPistolRound { get; set; }
    
    public bool IsEcoRound { get; set; }
    
    public bool IsForceRound { get; set; }
    
    public bool IsAntiEcoRound { get; set; }
    
    public bool IsClutchRound { get; set; }
    
    [MaxLength(100)]
    public string? ClutchPlayer { get; set; }
    
    public int ClutchPlayerCount { get; set; }
    
    public int ClutchEnemyCount { get; set; }
    
    public int CTPlayersAlive { get; set; }
    
    public int TPlayersAlive { get; set; }
    
    public int CTStartMoney { get; set; }
    
    public int TStartMoney { get; set; }
    
    public int CTEquipmentValue { get; set; }
    
    public int TEquipmentValue { get; set; }
    
    public bool BombPlanted { get; set; }
    
    public bool BombDefused { get; set; }
    
    public bool BombExploded { get; set; }
    
    [MaxLength(50)]
    public string? BombSite { get; set; }
    
    public int TotalKills { get; set; }
    
    public int HeadshotKills { get; set; }
    
    public int WallbangKills { get; set; }
    
    public int FlashAssists { get; set; }
    
    public double TotalDamage { get; set; }
    
    public double UtilityDamage { get; set; }
    
    [MaxLength(100)]
    public string? FirstKiller { get; set; }
    
    [MaxLength(100)]
    public string? FirstVictim { get; set; }
    
    public float FirstKillTime { get; set; }
    
    [MaxLength(100)]
    public string? FirstKillWeapon { get; set; }
    
    public bool FirstKillWasHeadshot { get; set; }
    
    public bool FirstKillWasWallbang { get; set; }
    
    public double FirstKillImpact { get; set; }
    
    [MaxLength(500)]
    public string? RoundNotes { get; set; }
}