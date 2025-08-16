using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerMovements")]
public class PlayerMovement
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
    public int PlayerId { get; set; }
    [ForeignKey("PlayerId")]
    public virtual Player Player { get; set; } = null!;
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    [Required]
    [StringLength(50)]
    public string MovementType { get; set; } = null!; // jump, land, crouch, uncrouch, walk, run, stop
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal VelocityX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal VelocityY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal VelocityZ { get; set; }
    
    public float Speed { get; set; }
    public float SpeedHorizontal { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal ViewAngleX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal ViewAngleY { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public bool IsOnGround { get; set; }
    public bool IsInAir { get; set; }
    public bool IsDucking { get; set; }
    public bool IsWalking { get; set; }
    
    public float? JumpHeight { get; set; }
    public float? FallDistance { get; set; }
    
    public bool IsBhopping { get; set; }
    public bool IsStrafing { get; set; }
    public bool IsSurfing { get; set; }
    
    [StringLength(255)]
    public string? MovementTechnique { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}