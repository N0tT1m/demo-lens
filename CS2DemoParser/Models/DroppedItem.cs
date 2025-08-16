using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("DroppedItems")]
public class DroppedItem
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
    
    public int? DropperPlayerId { get; set; }
    [ForeignKey("DropperPlayerId")]
    public virtual Player? DropperPlayer { get; set; }
    
    public int? PickerPlayerId { get; set; }
    [ForeignKey("PickerPlayerId")]
    public virtual Player? PickerPlayer { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ItemType { get; set; } = null!; // weapon, utility, armor, bomb
    
    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = null!;
    
    public int EntityId { get; set; } // Game engine entity ID
    
    // Drop event
    public int DropTick { get; set; }
    public float DropTime { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropVelocityX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropVelocityY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal DropVelocityZ { get; set; }
    
    [StringLength(50)]
    public string DropReason { get; set; } = null!; // death, manual, replace, disconnect
    
    // Pickup event (if applicable)
    public int? PickupTick { get; set; }
    public float? PickupTime { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? PickupPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? PickupPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? PickupPositionZ { get; set; }
    
    public float? TimeOnGround { get; set; } // Seconds between drop and pickup
    public bool WasPickedUp { get; set; }
    public bool Expired { get; set; } // Item disappeared without being picked up
    
    // Item state
    public int AmmoClip { get; set; }
    public int AmmoReserve { get; set; }
    public float Durability { get; set; }
    public int Value { get; set; } // Economic value
    
    // Weapon-specific properties
    public int Quality { get; set; }
    public float FloatValue { get; set; }
    [StringLength(100)]
    public string? SkinName { get; set; }
    public bool IsStattrak { get; set; }
    
    [StringLength(50)]
    public string? DropperTeam { get; set; }
    [StringLength(50)]
    public string? PickerTeam { get; set; }
    
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Properties { get; set; } // JSON for additional item data
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}