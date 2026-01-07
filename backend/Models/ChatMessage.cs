using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MatchISportsAPI.Models;

[Table("chat_messages")]
public class ChatMessage
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

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("isRead")]
    public bool IsRead { get; set; } = false;

    [Column("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("SenderId")]
    public User Sender { get; set; } = null!;

    [ForeignKey("ReceiverId")]
    public User Receiver { get; set; } = null!;
}
