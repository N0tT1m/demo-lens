using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("GameEvents")]
public class GameEvent
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? EventData { get; set; }
    
    [ForeignKey(nameof(Player))]
    public int? PlayerId { get; set; }
    public virtual Player? Player { get; set; }
    
    [MaxLength(100)]
    public string? PlayerName { get; set; }
    
    [MaxLength(50)]
    public string? Team { get; set; }
    
    public int? RoundNumber { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsImportant { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PositionZ { get; set; }
    
    [MaxLength(100)]
    public string? SubCategory { get; set; }
    
    public int? Value1 { get; set; }
    
    public int? Value2 { get; set; }
    
    public int? Value3 { get; set; }
    
    [MaxLength(100)]
    public string? StringValue1 { get; set; }
    
    [MaxLength(100)]
    public string? StringValue2 { get; set; }
    
    [MaxLength(100)]
    public string? StringValue3 { get; set; }
    
    public float? FloatValue1 { get; set; }
    
    public float? FloatValue2 { get; set; }
    
    public float? FloatValue3 { get; set; }
    
    public bool? BoolValue1 { get; set; }
    
    public bool? BoolValue2 { get; set; }
    
    public bool? BoolValue3 { get; set; }
    
    public DateTime CreatedAt { get; set; }
}