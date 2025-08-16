using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EntityVisibilities")]
public class EntityVisibility
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
    
    [Required]
    public int ObserverPlayerId { get; set; }
    [ForeignKey("ObserverPlayerId")]
    public virtual Player ObserverPlayer { get; set; } = null!;
    
    public int? TargetPlayerId { get; set; }
    [ForeignKey("TargetPlayerId")]
    public virtual Player? TargetPlayer { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = null!; // player, weapon, grenade, bomb, etc.
    
    [StringLength(100)]
    public string? EntityName { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    // Observer position and angles
    [Column(TypeName = "decimal(18,6)")]
    public decimal ObserverPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ObserverPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ObserverPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ObserverViewAngleX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ObserverViewAngleY { get; set; }
    
    // Target position
    [Column(TypeName = "decimal(18,6)")]
    public decimal TargetPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal TargetPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal TargetPositionZ { get; set; }
    
    public bool IsVisible { get; set; }
    public bool HasLineOfSight { get; set; }
    public bool IsInFieldOfView { get; set; }
    
    public float Distance { get; set; }
    public float ViewAngle { get; set; } // Angle from observer's view direction to target
    
    [StringLength(100)]
    public string? ObstructionType { get; set; } // wall, smoke, flash, etc.
    
    public float? VisibilityPercentage { get; set; } // How much of the target is visible (0-100)
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}