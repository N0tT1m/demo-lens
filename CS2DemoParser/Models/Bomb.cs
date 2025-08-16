using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("Bombs")]
public class Bomb
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Round))]
    public int RoundId { get; set; }
    public virtual Round Round { get; set; } = null!;
    
    [MaxLength(50)]
    public string? EventType { get; set; }
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [ForeignKey(nameof(Player))]
    public int? PlayerId { get; set; }
    public virtual Player? Player { get; set; }
    
    [MaxLength(10)]
    public string? Site { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionX { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionY { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PositionZ { get; set; }
    
    public float? PlantTime { get; set; }
    
    public float? DefuseTime { get; set; }
    
    public float? ExplodeTime { get; set; }
    
    public bool HasKit { get; set; }
    
    public bool IsDefuseStarted { get; set; }
    
    public bool IsDefuseCancelled { get; set; }
    
    public bool IsPlantStarted { get; set; }
    
    public bool IsPlantCancelled { get; set; }
    
    public int CTPlayersInRange { get; set; }
    
    public int TPlayersInRange { get; set; }
    
    public float DefuseProgress { get; set; }
    
    public float PlantProgress { get; set; }
    
    public float TimeRemaining { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public bool IsClutch { get; set; }
    
    public int ClutchSize { get; set; }
    
    public bool HasSmoke { get; set; }
    
    public bool HasFlash { get; set; }
    
    public bool UnderFire { get; set; }
    
    public float NearestEnemyDistance { get; set; }
    
    public int RoundNumber { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}