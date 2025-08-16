using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("MapControls")]
public class MapControl
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
    
    public int Tick { get; set; }
    public float GameTime { get; set; }
    
    public int RoundNumber { get; set; }
    
    [Required]
    [StringLength(100)]
    public string MapName { get; set; } = null!;
    
    // Area control percentages (0-100)
    public float CTAreaControl { get; set; }
    public float TAreaControl { get; set; }
    public float NeutralAreaControl { get; set; }
    
    // Site-specific control
    public float ASiteControl { get; set; } // Who controls A site (-100 to +100, negative = T control, positive = CT control)
    public float BSiteControl { get; set; } // Who controls B site
    public float MidControl { get; set; } // Mid area control
    
    // Key area presence
    public int CTPlayersInAsite { get; set; }
    public int TPlayersInAsite { get; set; }
    public int CTPlayersInBsite { get; set; }
    public int TPlayersInBsite { get; set; }
    public int CTPlayersInMid { get; set; }
    public int TPlayersInMid { get; set; }
    
    // Choke point control
    [StringLength(255)]
    public string? ControlledChokes { get; set; } // JSON array of controlled choke points
    
    // Tactical positions
    public int AdvantageousPositions { get; set; } // Number of players in good positions
    public int DisadvantageousPositions { get; set; } // Number of players in bad positions
    
    // Utility usage for area control
    public int SmokesCoveringAreas { get; set; }
    public int FlashesBlindingAreas { get; set; }
    public int HEGrenadesControllingAreas { get; set; }
    public int MolotovsBlockingAreas { get; set; }
    
    // Dynamic control metrics
    public float ControlMomentum { get; set; } // Rate of change in control (-100 to +100)
    public bool IsShiftingControl { get; set; } // If control is actively changing
    public float TimeInControl { get; set; } // How long current control has been maintained
    
    // Map-specific zones (these would be map-dependent)
    [StringLength(100)]
    public string? DominantTeamZone { get; set; } // Which team controls more territory
    public float TerritoryBalance { get; set; } // How balanced the territorial control is
    
    // Rotation indicators
    public bool CTRotatingToA { get; set; }
    public bool CTRotatingToB { get; set; }
    public bool TRotatingToA { get; set; }
    public bool TRotatingToB { get; set; }
    
    // Strategic implications
    public bool CTStackedOneSite { get; set; }
    public bool TCommittedToSite { get; set; }
    [StringLength(100)]
    public string? ExpectedStrategy { get; set; } // Predicted team strategy based on positions
    
    [StringLength(255)]
    public string? ControlNotes { get; set; } // Additional observations about map control
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}