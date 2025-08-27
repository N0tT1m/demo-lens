using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("PlayerInputs")]
public class PlayerInput
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Round))]
    public int RoundId { get; set; }
    public virtual Round Round { get; set; } = null!;
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    // Input button states from demofile-net InputButtons enum
    public bool Forward { get; set; }        // W key
    public bool Backward { get; set; }       // S key
    public bool Left { get; set; }           // A key  
    public bool Right { get; set; }          // D key
    public bool Jump { get; set; }           // Space key
    public bool Duck { get; set; }           // Ctrl key
    public bool Attack { get; set; }         // Left mouse
    public bool Attack2 { get; set; }        // Right mouse
    public bool Reload { get; set; }         // R key
    public bool Use { get; set; }            // E key
    public bool Walk { get; set; }           // Shift key (walk)
    public bool Speed { get; set; }          // Shift key (run)
    
    // Position and view angles at time of input
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ViewAngleY { get; set; }
    
    // Movement analysis
    public float Velocity { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public float VelocityZ { get; set; }
    
    // Advanced movement detection
    public bool IsCounterStrafing { get; set; }
    public bool IsPeeking { get; set; }
    public bool IsJigglePeeking { get; set; }
    public bool IsBhopping { get; set; }
    
    [MaxLength(50)]
    public string? MovementType { get; set; }  // "Strafe", "Peek", "Hold", "Rotate", etc.
}