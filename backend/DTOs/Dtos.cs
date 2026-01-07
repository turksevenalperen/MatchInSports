using MatchISportsAPI.Models;

namespace MatchISportsAPI.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Bio { get; set; }
    public string? Logo { get; set; }
    public bool IsPro { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    public string? TeamName { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }
    public string? Logo { get; set; }
}

public class CreateMatchDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
}

public class MatchDto
{
    public string Id { get; set; } = string.Empty;
    public string CreatorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserDto? Creator { get; set; }
}

public class TeamRequestDto
{
    public string ReceiverId { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class UpdateRequestStatusDto
{
    public string Status { get; set; } = string.Empty; // ACCEPTED or REJECTED
}

public class ChatMessageDto
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class CreateMatchHistoryDto
{
    public string Team2Id { get; set; } = string.Empty;
    public string Team2Name { get; set; } = string.Empty;
    public int? Score1 { get; set; }
    public int? Score2 { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? Rating1 { get; set; }
    public int? Rating2 { get; set; }
}
