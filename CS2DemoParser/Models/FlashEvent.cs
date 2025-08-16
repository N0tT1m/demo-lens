using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("FlashEvents")]
public class FlashEvent
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
    public int FlashedPlayerId { get; set; }
    [ForeignKey("FlashedPlayerId")]
    public virtual Player FlashedPlayer { get; set; } = null!;
    
    public int? FlasherPlayerId { get; set; }
    [ForeignKey("FlasherPlayerId")]
    public virtual Player? FlasherPlayer { get; set; }
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    public float FlashDuration { get; set; }
    public float FlashAlpha { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal FlashedPlayerPositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal FlashedPlayerPositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal FlashedPlayerPositionZ { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? GrenadePositionX { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? GrenadePositionY { get; set; }
    [Column(TypeName = "decimal(18,6)")]
    public decimal? GrenadePositionZ { get; set; }
    
    public float? Distance { get; set; }
    
    [StringLength(50)]
    public string FlashedPlayerTeam { get; set; } = null!;
    [StringLength(50)]
    public string? FlasherPlayerTeam { get; set; }
    
    public bool IsTeamFlash { get; set; }
    public bool IsSelfFlash { get; set; }
    
    public int RoundNumber { get; set; }
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}