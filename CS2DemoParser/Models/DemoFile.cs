using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("DemoFiles")]
public class DemoFile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime ParsedAt { get; set; }
    
    [MaxLength(100)]
    public string? MapName { get; set; }
    
    [MaxLength(50)]
    public string? GameMode { get; set; }
    
    public int TickRate { get; set; }
    
    public int TotalTicks { get; set; }
    
    public float Duration { get; set; }
    
    [MaxLength(50)]
    public string? ServerName { get; set; }
    
    [MaxLength(500)]
    public string? DemoType { get; set; }
    
    [MaxLength(50)]
    public string? DemoSource { get; set; } // matchmaking, faceit, esea, other
    
    public int NetworkProtocol { get; set; }
    
    [MaxLength(100)]
    public string? ClientName { get; set; }
    
    [MaxLength(500)]
    public string? SignonState { get; set; }
    
    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    public virtual ICollection<Round> Rounds { get; set; } = new List<Round>();
    public virtual ICollection<GameEvent> GameEvents { get; set; } = new List<GameEvent>();
    public virtual ICollection<Kill> Kills { get; set; } = new List<Kill>();
    public virtual ICollection<Damage> Damages { get; set; } = new List<Damage>();
    public virtual ICollection<WeaponFire> WeaponFires { get; set; } = new List<WeaponFire>();
    public virtual ICollection<Grenade> Grenades { get; set; } = new List<Grenade>();
    public virtual ICollection<Bomb> Bombs { get; set; } = new List<Bomb>();
    public virtual ICollection<PlayerPosition> PlayerPositions { get; set; } = new List<PlayerPosition>();
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}