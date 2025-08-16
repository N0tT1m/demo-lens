using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CS2DemoParser.Models;

[Table("ChatMessages")]
public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(DemoFile))]
    public int DemoFileId { get; set; }
    public virtual DemoFile DemoFile { get; set; } = null!;
    
    [ForeignKey(nameof(Player))]
    public int? PlayerId { get; set; }
    public virtual Player? Player { get; set; }
    
    public int Tick { get; set; }
    
    public float GameTime { get; set; }
    
    [MaxLength(1000)]
    public string? Message { get; set; }
    
    [MaxLength(100)]
    public string? SenderName { get; set; }
    
    [MaxLength(100)]
    public string? Team { get; set; }
    
    public bool IsTeamMessage { get; set; }
    
    public bool IsAllChat { get; set; }
    
    public bool IsDeadChat { get; set; }
    
    public bool IsSystemMessage { get; set; }
    
    public bool IsRadioMessage { get; set; }
    
    [MaxLength(50)]
    public string? MessageType { get; set; }
    
    [MaxLength(100)]
    public string? RadioCommand { get; set; }
    
    public bool IsMuted { get; set; }
    
    public bool IsSpam { get; set; }
    
    public DateTime Timestamp { get; set; }
}