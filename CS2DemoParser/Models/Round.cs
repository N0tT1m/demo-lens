using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Rounds")]
public class Round
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Match))]
    public int MatchId { get; set; }
    public virtual Match Match { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    public int StartTick { get; set; }
    
    public int? EndTick { get; set; }
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public float Duration { get; set; }
    
    [MaxLength(50)]
    public string? WinnerTeam { get; set; }
    
    [MaxLength(100)]
    public string? EndReason { get; set; }
    
    public int CTScore { get; set; }
    
    public int TScore { get; set; }
    
    public int CTLivePlayers { get; set; }
    
    public int TLivePlayers { get; set; }
    
    public int CTStartMoney { get; set; }
    
    public int TStartMoney { get; set; }
    
    public int CTEquipmentValue { get; set; }
    
    public int TEquipmentValue { get; set; }
    
    public bool BombPlanted { get; set; }
    
    public bool BombDefused { get; set; }
    
    public bool BombExploded { get; set; }
    
    public int? BombSite { get; set; }
    
    public bool IsEcoRound { get; set; }
    
    public bool IsForceBuyRound { get; set; }
    
    public bool IsAntiEcoRound { get; set; }
    
    public bool IsPistolRound { get; set; }
    
    public bool IsOvertime { get; set; }
    
    public virtual ICollection<PlayerRoundStats> PlayerRoundStats { get; set; } = new List<PlayerRoundStats>();
    public virtual ICollection<Kill> Kills { get; set; } = new List<Kill>();
    public virtual ICollection<Damage> Damages { get; set; } = new List<Damage>();
    public virtual ICollection<WeaponFire> WeaponFires { get; set; } = new List<WeaponFire>();
    public virtual ICollection<Grenade> Grenades { get; set; } = new List<Grenade>();
    public virtual ICollection<Bomb> Bombs { get; set; } = new List<Bomb>();
}