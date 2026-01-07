using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MatchISportsAPI.Data;
using MatchISportsAPI.Models;
using MatchISportsAPI.DTOs;

namespace MatchISportsAPI.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessageController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("messages")]
    public async Task<IActionResult> GetChatMessages([FromQuery] string? userId)
    {
        var currentUserId = GetUserId();

        var query = _context.ChatMessages
            .Include(cm => cm.Sender)
            .Include(cm => cm.Receiver)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(cm => 
                (cm.SenderId == currentUserId && cm.ReceiverId == userId) ||
                (cm.SenderId == userId && cm.ReceiverId == currentUserId));
        }
        else
        {
            query = query.Where(cm => 
                cm.SenderId == currentUserId || cm.ReceiverId == currentUserId);
        }

        var messages = await query
            .OrderBy(cm => cm.CreatedAt)
            .Select(cm => new
            {
                cm.Id,
                cm.SenderId,
                cm.ReceiverId,
                cm.Content,
                cm.IsRead,
                cm.CreatedAt,
                Sender = new UserDto
                {
                    Id = cm.Sender.Id,
                    TeamName = cm.Sender.TeamName,
                    Logo = cm.Sender.Logo,
                    Email = cm.Sender.Email,
                    City = cm.Sender.City,
                    Sport = cm.Sender.Sport.ToString(),
                    Rating = cm.Sender.Rating,
                    Bio = cm.Sender.Bio,
                    IsPro = cm.Sender.IsPro,
                    CreatedAt = cm.Sender.CreatedAt
                },
                Receiver = new UserDto
                {
                    Id = cm.Receiver.Id,
                    TeamName = cm.Receiver.TeamName,
                    Logo = cm.Receiver.Logo,
                    Email = cm.Receiver.Email,
                    City = cm.Receiver.City,
                    Sport = cm.Receiver.Sport.ToString(),
                    Rating = cm.Receiver.Rating,
                    Bio = cm.Receiver.Bio,
                    IsPro = cm.Receiver.IsPro,
                    CreatedAt = cm.Receiver.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendChatMessage([FromBody] ChatMessageDto dto)
    {
        var userId = GetUserId();

        var message = new ChatMessage
        {
            SenderId = userId,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);

        // Create activity
        var activity = new Activity
        {
            UserId = userId,
            Type = ActivityType.CHAT_STARTED,
            Title = "Sohbet Başladı",
            Description = "Yeni bir sohbet başlattın",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Message sent successfully", id = message.Id });
    }

    [HttpGet("match-history")]
    public async Task<IActionResult> GetMatchHistory()
    {
        var userId = GetUserId();

        var history = await _context.MatchHistories
            .Include(mh => mh.Team1)
            .Include(mh => mh.Team2)
            .Where(mh => mh.Team1Id == userId || mh.Team2Id == userId)
            .OrderByDescending(mh => mh.Date)
            .Select(mh => new
            {
                mh.Id,
                mh.Team1Id,
                mh.Team2Id,
                mh.Team1Name,
                mh.Team2Name,
                mh.Score1,
                mh.Score2,
                mh.Location,
                mh.Date,
                mh.Rating1,
                mh.Rating2,
                Team1 = new UserDto
                {
                    Id = mh.Team1.Id,
                    TeamName = mh.Team1.TeamName,
                    Logo = mh.Team1.Logo,
                    Rating = mh.Team1.Rating,
                    Email = mh.Team1.Email,
                    City = mh.Team1.City,
                    Sport = mh.Team1.Sport.ToString(),
                    Bio = mh.Team1.Bio,
                    IsPro = mh.Team1.IsPro,
                    CreatedAt = mh.Team1.CreatedAt
                },
                Team2 = new UserDto
                {
                    Id = mh.Team2.Id,
                    TeamName = mh.Team2.TeamName,
                    Logo = mh.Team2.Logo,
                    Rating = mh.Team2.Rating,
                    Email = mh.Team2.Email,
                    City = mh.Team2.City,
                    Sport = mh.Team2.Sport.ToString(),
                    Bio = mh.Team2.Bio,
                    IsPro = mh.Team2.IsPro,
                    CreatedAt = mh.Team2.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpPost("match-history")]
    public async Task<IActionResult> CreateMatchHistory([FromBody] CreateMatchHistoryDto dto)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        var history = new MatchHistory
        {
            Team1Id = userId,
            Team2Id = dto.Team2Id,
            Team1Name = user.TeamName,
            Team2Name = dto.Team2Name,
            Score1 = dto.Score1,
            Score2 = dto.Score2,
            Location = dto.Location,
            Date = dto.Date,
            Rating1 = dto.Rating1,
            Rating2 = dto.Rating2,
            CreatedAt = DateTime.UtcNow
        };

        _context.MatchHistories.Add(history);

        // Create activities based on match result
        if (dto.Score1.HasValue && dto.Score2.HasValue)
        {
            var activityType = dto.Score1 > dto.Score2 
                ? ActivityType.MATCH_WON 
                : ActivityType.MATCH_LOST;

            var activity = new Activity
            {
                UserId = userId,
                Type = activityType,
                Title = activityType == ActivityType.MATCH_WON ? "Maç Kazandın" : "Maç Kaybettin",
                Description = $"{dto.Team2Name} ile maç {dto.Score1}-{dto.Score2} bitti",
                CreatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Match history created successfully" });
    }
}
