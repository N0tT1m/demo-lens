using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("EconomyEvents")]
public class EconomyEvent
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
    public string EventType { get; set; } = null!; // purchase, pickup, drop, lose, adjust
    
    [StringLength(100)]
    public string? ItemName { get; set; }
    
    [StringLength(50)]
    public string? ItemCategory { get; set; } // weapon, grenade, armor, utility
    
    public int? ItemCost { get; set; }
    public int MoneyBefore { get; set; }
    public int MoneyAfter { get; set; }
    public int MoneyChange { get; set; }
    
    public int? ItemQuantity { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    public bool IsInBuyZone { get; set; }
    public bool IsBuyTimeActive { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}