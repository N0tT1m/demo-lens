using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EntityInteractions")]
public class EntityInteraction
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
    
    public int? InitiatorPlayerId { get; set; }
    [ForeignKey("InitiatorPlayerId")]
    public virtual Player? InitiatorPlayer { get; set; }
    
    public int? TargetPlayerId { get; set; }
    [ForeignKey("TargetPlayerId")]
    public virtual Player? TargetPlayer { get; set; }
    
    [Required]
    [StringLength(50)]
    public string InteractionType { get; set; } = null!; // pickup, transfer, collision, impact, use
    
    [Required]
    [StringLength(100)]
    public string SourceEntityType { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string SourceEntityName { get; set; } = null!;
    
    [StringLength(100)]
    public string? TargetEntityType { get; set; }
    [StringLength(100)]
    public string? TargetEntityName { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    public float? Distance { get; set; }
    public float? Angle { get; set; }
    public float? Force { get; set; }
    
    [StringLength(255)]
    public string? Result { get; set; } // Outcome of the interaction
    
    public bool IsSuccessful { get; set; }
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON for additional data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}