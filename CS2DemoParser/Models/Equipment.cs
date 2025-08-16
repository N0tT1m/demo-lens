using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Equipment")]
public class Equipment
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = null!;
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [MaxLength(100)]
    public string? ItemName { get; set; }
    
    [MaxLength(50)]
    public string? ItemType { get; set; }
    
    [MaxLength(50)]
    public string? ItemCategory { get; set; }
    
    [MaxLength(50)]
    public string? Action { get; set; }
    
    public int Cost { get; set; }
    
    public int Ammo { get; set; }
    
    public int AmmoReserve { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool IsDropped { get; set; }
    
    public bool IsPurchased { get; set; }
    
    public bool IsPickedUp { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionZ { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public int RoundNumber { get; set; }
    
    public int Quality { get; set; }
    
    public int Wear { get; set; }
    
    [MaxLength(100)]
    public string? SkinName { get; set; }
    
    [MaxLength(50)]
    public string? StatTrak { get; set; }
    
    public bool IsStattrak { get; set; }
    
    public bool IsSouvenir { get; set; }
    
    public float FloatValue { get; set; }
    
    public int PaintSeed { get; set; }
    
    public int ItemId { get; set; }
    
    public ulong InventoryId { get; set; }
    
    public ulong AccountId { get; set; }
    
    [MaxLength(100)]
    public string? Origin { get; set; }
    
    public bool IsDefault { get; set; }
    
    public int Stickers { get; set; }
    
    [MaxLength(500)]
    public string? StickerInfo { get; set; }
    
    public bool IsNameTag { get; set; }
    
    [MaxLength(100)]
    public string? CustomName { get; set; }
}