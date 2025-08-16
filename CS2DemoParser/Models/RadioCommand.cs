using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("RadioCommands")]
public class RadioCommand
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
    [StringLength(100)]
    public string Command { get; set; } = null!;
    
    [StringLength(50)]
    public string CommandCategory { get; set; } = null!; // tactical, communication, response, etc.
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Context { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}