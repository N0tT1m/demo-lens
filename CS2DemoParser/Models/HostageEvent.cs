using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("HostageEvents")]
public class HostageEvent
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
    public string EventType { get; set; } = null!; // follows, hurt, killed, rescued, stops_following, call_for_help
    
    public int HostageEntityId { get; set; } // Hostage entity index
    
    [StringLength(50)]
    public string? HostageName { get; set; } // Hostage identifier
    
    // Position data
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    // Event-specific data
    public int? DamageDealt { get; set; } // For hurt events
    public int? HealthRemaining { get; set; } // Health after event
    
    [StringLength(50)]
    public string? AttackerWeapon { get; set; } // Weapon used to hurt/kill hostage
    
    public bool IsHeadshot { get; set; } // For hurt/killed events
    
    // Rescue-specific data
    public float? RescueTime { get; set; } // Time taken to rescue
    public float? DistanceToRescueZone { get; set; } // Distance when rescued
    
    // Following behavior data
    public float? FollowDuration { get; set; } // How long hostage followed player
    public float? FollowDistance { get; set; } // Distance maintained while following
    
    // Hostage state
    [StringLength(50)]
    public string? HostageState { get; set; } // idle, following, being_rescued, dead, etc.
    
    public bool WasBeingFollowed { get; set; }
    public bool WasBeingRescued { get; set; }
    
    // Round impact
    public bool IsRoundWinning { get; set; } // If this event won the round
    public bool IsLastHostage { get; set; } // If this was the last hostage
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}