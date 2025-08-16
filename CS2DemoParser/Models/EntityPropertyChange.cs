using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EntityPropertyChanges")]
public class EntityPropertyChange
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
    public int EntityIndex { get; set; } // Entity index in demo
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = null!; // CCSPlayerController, CCSPlayerPawn, etc.
    
    [Required]
    [StringLength(100)]
    public string PropertyName { get; set; } = null!; // Health, Armor, Money, etc.
    
    [StringLength(255)]
    public string? OldValue { get; set; } // Previous value (serialized)
    
    [StringLength(255)]
    public string? NewValue { get; set; } // New value (serialized)
    
    [StringLength(50)]
    public string? ChangeType { get; set; } // increased, decreased, changed, set
    
    public float? NumericOldValue { get; set; } // For numeric properties
    public float? NumericNewValue { get; set; } // For numeric properties
    public float? ChangeDelta { get; set; } // Difference for numeric changes
    
    // Context information
    public bool IsSignificantChange { get; set; } // Major changes (death, large damage, etc.)
    public bool IsGameplayRelevant { get; set; } // Changes that affect gameplay
    
    [StringLength(100)]
    public string? ChangeContext { get; set; } // combat, economic, movement, etc.
    
    [StringLength(100)]
    public string? TriggerEvent { get; set; } // Event that caused the change
    
    public int? CausedByPlayerId { get; set; } // Player who caused this change
    [ForeignKey("CausedByPlayerId")]
    public virtual Player? CausedByPlayer { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? AdditionalData { get; set; } // JSON for complex property data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}