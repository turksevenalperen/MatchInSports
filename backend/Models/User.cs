using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column("teamName")]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    [Column("city")]
    public string City { get; set; } = string.Empty;

    [Required]
    [Column("sport")]
    public SportType Sport { get; set; }

    [Column("rating")]
    public int Rating { get; set; } = 50;

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("logo")]
    public string? Logo { get; set; }

    [Column("isPro")]
    public bool IsPro { get; set; } = false;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Match> CreatedMatches { get; set; } = new List<Match>();
    public ICollection<MatchRequest> MatchRequests { get; set; } = new List<MatchRequest>();
    public ICollection<TeamRequest> SentTeamRequests { get; set; } = new List<TeamRequest>();
    public ICollection<TeamRequest> ReceivedTeamRequests { get; set; } = new List<TeamRequest>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<ChatMessage> SentChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<MatchHistory> MatchHistory1 { get; set; } = new List<MatchHistory>();
    public ICollection<MatchHistory> MatchHistory2 { get; set; } = new List<MatchHistory>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public enum SportType
{
    FUTBOL,
    BASKETBOL,
    VOLEYBOL,
    TENIS,
    HENTBOL,
    BADMINTON
}
