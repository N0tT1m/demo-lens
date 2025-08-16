using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("WeaponStates")]
public class WeaponState
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
    public string EventType { get; set; } = null!; // reload, zoom, switch, draw, holster
    
    [Required]
    [StringLength(100)]
    public string WeaponName { get; set; } = null!;
    
    public int AmmoClip { get; set; }
    public int AmmoReserve { get; set; }
    
    public bool IsScoped { get; set; }
    public int ZoomLevel { get; set; }
    
    public bool IsSilenced { get; set; }
    public bool IsReloading { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal PositionZ { get; set; }
    
    [StringLength(50)]
    public string Team { get; set; } = null!;
    
    public int RoundNumber { get; set; }
    
    public float? ReloadDuration { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}