using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("match_requests")]
public class MatchRequest
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("matchId")]
    public string MatchId { get; set; } = string.Empty;

    [Required]
    [Column("requesterId")]
    public string RequesterId { get; set; } = string.Empty;

    [Column("status")]
    public RequestStatus Status { get; set; } = RequestStatus.PENDING;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("MatchId")]
    public Match Match { get; set; } = null!;

    [ForeignKey("RequesterId")]
    public User Requester { get; set; } = null!;
}

public enum RequestStatus
{
    PENDING,
    ACCEPTED,
    REJECTED
}
