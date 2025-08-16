using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EntityLifecycles")]
public class EntityLifecycle
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
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = null!; // weapon, grenade, bomb, hostage, environmental
    
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = null!; // specific entity identifier
    
    [Required]
    [StringLength(50)]
    public string EventType { get; set; } = null!; // spawn, pickup, drop, destroy, expire
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    [StringLength(50)]
    public string? Team { get; set; }
    
    public int RoundNumber { get; set; }
    
    // Entity properties
    public int EntityId { get; set; } // Game engine entity ID
    public bool IsActive { get; set; }
    public float? Durability { get; set; }
    public int? Value { get; set; } // Economic value
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON string for additional properties
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}