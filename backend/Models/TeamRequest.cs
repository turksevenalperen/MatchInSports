using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("team_requests")]
public class TeamRequest
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("senderId")]
    public string SenderId { get; set; } = string.Empty;

    [Required]
    [Column("receiverId")]
    public string ReceiverId { get; set; } = string.Empty;

    [Column("message")]
    public string? Message { get; set; }

    [Column("status")]
    public RequestStatus Status { get; set; } = RequestStatus.PENDING;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("SenderId")]
    public User Sender { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    public User Receiver { get; set; } = null!;
}
