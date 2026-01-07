using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("matches")]
public class Match
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("creatorId")]
    public string CreatorId { get; set; } = string.Empty;

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("date")]
    public DateTime Date { get; set; }

    [Required]
    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Column("sport")]
    public SportType Sport { get; set; }

    [Column("status")]
    public MatchStatus Status { get; set; } = MatchStatus.ACTIVE;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("CreatorId")]
    public User Creator { get; set; } = null!;
    public ICollection<MatchRequest> Requests { get; set; } = new List<MatchRequest>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public enum MatchStatus
{
    ACTIVE,
    MATCHED,
    COMPLETED,
    CANCELLED
}
