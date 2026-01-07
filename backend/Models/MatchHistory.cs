using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("match_history")]
public class MatchHistory
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [Column("team1Id")]
    public string Team1Id { get; set; } = string.Empty;

    [Required]
    [Column("team2Id")]
    public string Team2Id { get; set; } = string.Empty;

    [Required]
    [Column("team1Name")]
    public string Team1Name { get; set; } = string.Empty;

    [Required]
    [Column("team2Name")]
    public string Team2Name { get; set; } = string.Empty;

    [Column("score1")]
    public int? Score1 { get; set; }

    [Column("score2")]
    public int? Score2 { get; set; }

    [Required]
    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Column("date")]
    public DateTime Date { get; set; }

    [Column("rating1")]
    public int? Rating1 { get; set; }

    [Column("rating2")]
    public int? Rating2 { get; set; }

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("Team1Id")]
    public User Team1 { get; set; } = null!;

    [ForeignKey("Team2Id")]
    public User Team2 { get; set; } = null!;
}
