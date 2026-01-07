using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MatchISportsAPI.Data;
using MatchISportsAPI.Models;
using MatchISportsAPI.DTOs;

namespace MatchISportsAPI.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeamController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> SearchTeams(
        [FromQuery] string? sport,
        [FromQuery] string? city,
        [FromQuery] string? search)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(sport) && Enum.TryParse<SportType>(sport.ToUpper(), out var sportType))
        {
            query = query.Where(u => u.Sport == sportType);
        }

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(u => u.City.Contains(city));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.TeamName.Contains(search) || u.City.Contains(search));
        }

        var teams = await query
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                TeamName = u.TeamName,
                City = u.City,
                Sport = u.Sport.ToString(),
                Rating = u.Rating,
                Bio = u.Bio,
                Logo = u.Logo,
                IsPro = u.IsPro,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetTeam(string id)
    {
        var team = await _context.Users.FindAsync(id);

        if (team == null)
            return NotFound();

        return Ok(new UserDto
        {
            Id = team.Id,
            Email = team.Email,
            TeamName = team.TeamName,
            City = team.City,
            Sport = team.Sport.ToString(),
            Rating = team.Rating,
            Bio = team.Bio,
            Logo = team.Logo,
            IsPro = team.IsPro,
            CreatedAt = team.CreatedAt
        });
    }

    [HttpPost("request")]
    public async Task<IActionResult> CreateTeamRequest([FromBody] TeamRequestDto dto)
    {
        var userId = GetUserId();

        // Check if request already exists
        var existingRequest = await _context.TeamRequests
            .FirstOrDefaultAsync(tr => tr.SenderId == userId && tr.ReceiverId == dto.ReceiverId);

        if (existingRequest != null)
            return BadRequest(new { message = "Request already sent" });

        var request = new TeamRequest
        {
            SenderId = userId,
            ReceiverId = dto.ReceiverId,
            Message = dto.Message,
            Status = RequestStatus.PENDING,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TeamRequests.Add(request);

        // Create activity for sender
        var senderActivity = new Activity
        {
            UserId = userId,
            Type = ActivityType.REQUEST_SENT,
            Title = "Takım İsteği Gönderdin",
            Description = "Bir takıma istek gönderdin",
            CreatedAt = DateTime.UtcNow
        };

        // Create activity for receiver
        var receiverActivity = new Activity
        {
            UserId = dto.ReceiverId,
            Type = ActivityType.REQUEST_RECEIVED,
            Title = "Takım İsteği Aldın",
            Description = "Bir takımdan istek aldın",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(senderActivity);
        _context.Activities.Add(receiverActivity);
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "Request sent successfully" });
    }

    [HttpGet("incoming-requests")]
    public async Task<IActionResult> GetIncomingRequests()
    {
        var userId = GetUserId();

        var requests = await _context.TeamRequests
            .Include(tr => tr.Sender)
            .Where(tr => tr.ReceiverId == userId)
            .OrderByDescending(tr => tr.CreatedAt)
            .Select(tr => new
            {
                tr.Id,
                tr.Message,
                tr.Status,
                tr.CreatedAt,
                Sender = new UserDto
                {
                    Id = tr.Sender.Id,
                    TeamName = tr.Sender.TeamName,
                    City = tr.Sender.City,
                    Logo = tr.Sender.Logo,
                    Rating = tr.Sender.Rating,
                    Email = tr.Sender.Email,
                    Sport = tr.Sender.Sport.ToString(),
                    Bio = tr.Sender.Bio,
                    IsPro = tr.Sender.IsPro,
                    CreatedAt = tr.Sender.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests()
    {
        var userId = GetUserId();

        var requests = await _context.TeamRequests
            .Include(tr => tr.Receiver)
            .Where(tr => tr.SenderId == userId)
            .OrderByDescending(tr => tr.CreatedAt)
            .Select(tr => new
            {
                tr.Id,
                tr.Message,
                tr.Status,
                tr.CreatedAt,
                Receiver = new UserDto
                {
                    Id = tr.Receiver.Id,
                    TeamName = tr.Receiver.TeamName,
                    City = tr.Receiver.City,
                    Logo = tr.Receiver.Logo,
                    Rating = tr.Receiver.Rating,
                    Email = tr.Receiver.Email,
                    Sport = tr.Receiver.Sport.ToString(),
                    Bio = tr.Receiver.Bio,
                    IsPro = tr.Receiver.IsPro,
                    CreatedAt = tr.Receiver.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPut("{id}/request")]
    public async Task<IActionResult> UpdateRequestStatus(string id, [FromBody] UpdateRequestStatusDto dto)
    {
        var userId = GetUserId();

        var request = await _context.TeamRequests
            .Include(tr => tr.Sender)
            .FirstOrDefaultAsync(tr => tr.Id == id && tr.ReceiverId == userId);

        if (request == null)
            return NotFound();

        if (!Enum.TryParse<RequestStatus>(dto.Status.ToUpper(), out var status))
        {
            return BadRequest(new { message = "Invalid status" });
        }

        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;

        // Create activities
        var activityType = status == RequestStatus.ACCEPTED 
            ? ActivityType.REQUEST_ACCEPTED 
            : ActivityType.REQUEST_REJECTED;

        var senderActivity = new Activity
        {
            UserId = request.SenderId,
            Type = activityType,
            Title = status == RequestStatus.ACCEPTED ? "İsteğin Kabul Edildi" : "İsteğin Reddedildi",
            Description = $"Takım isteğin {(status == RequestStatus.ACCEPTED ? "kabul edildi" : "reddedildi")}",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(senderActivity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Request updated successfully" });
    }
}
