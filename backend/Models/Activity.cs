using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("activities")]
public class Activity
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("userId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column("type")]
    public ActivityType Type { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}

public enum ActivityType
{
    PLATFORM_JOIN,
    REQUEST_SENT,
    REQUEST_RECEIVED,
    REQUEST_ACCEPTED,
    REQUEST_REJECTED,
    MATCH_CREATED,
    CHAT_STARTED,
    MATCH_WON,
    MATCH_LOST
}
