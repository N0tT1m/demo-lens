using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Players")]
public class Player
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    public int PlayerSlot { get; set; }
    
    public int UserId { get; set; }
    
    public ulong SteamId { get; set; }
    
    [MaxLength(100)]
    public string? PlayerName { get; set; }
    
    [MaxLength(50)]
    public string? Team { get; set; }
    
    public bool IsBot { get; set; }
    
    public bool IsHltv { get; set; }
    
    public bool IsConnected { get; set; }
    
    public int Rank { get; set; }
    
    public int Wins { get; set; }
    
    [MaxLength(200)]
    public string? ClanTag { get; set; }
    
    public DateTime? ConnectedAt { get; set; }
    
    public DateTime? DisconnectedAt { get; set; }
    
    [MaxLength(500)]
    public string? DisconnectReason { get; set; }
    
    public int PingAverage { get; set; }
    
    public int PingMax { get; set; }
    
    public int PingMin { get; set; }
    
    public float PacketLoss { get; set; }
    
    public virtual ICollection<PlayerMatchStats> PlayerMatchStats { get; set; } = new List<PlayerMatchStats>();
    public virtual ICollection<PlayerRoundStats> PlayerRoundStats { get; set; } = new List<PlayerRoundStats>();
    public virtual ICollection<Kill> KillsAsKiller { get; set; } = new List<Kill>();
    public virtual ICollection<Kill> KillsAsVictim { get; set; } = new List<Kill>();
    public virtual ICollection<Kill> KillsAsAssister { get; set; } = new List<Kill>();
    public virtual ICollection<Damage> DamagesAsAttacker { get; set; } = new List<Damage>();
    public virtual ICollection<Damage> DamagesAsVictim { get; set; } = new List<Damage>();
    public virtual ICollection<WeaponFire> WeaponFires { get; set; } = new List<WeaponFire>();
    public virtual ICollection<PlayerPosition> PlayerPositions { get; set; } = new List<PlayerPosition>();
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
    public virtual ICollection<Grenade> Grenades { get; set; } = new List<Grenade>();
}