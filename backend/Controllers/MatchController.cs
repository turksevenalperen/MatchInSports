using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MatchISportsAPI.Data;
using MatchISportsAPI.Models;
using MatchISportsAPI.DTOs;

namespace MatchISportsAPI.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MatchController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches(
        [FromQuery] string? sport, 
        [FromQuery] string? city,
        [FromQuery] string? status)
    {
        var query = _context.Matches
            .Include(m => m.Creator)
            .AsQueryable();

        if (!string.IsNullOrEmpty(sport) && Enum.TryParse<SportType>(sport.ToUpper(), out var sportType))
        {
            query = query.Where(m => m.Sport == sportType);
        }

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(m => m.Location.Contains(city));
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MatchStatus>(status.ToUpper(), out var matchStatus))
        {
            query = query.Where(m => m.Status == matchStatus);
        }
        else
        {
            query = query.Where(m => m.Status == MatchStatus.ACTIVE);
        }

        var matches = await query
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MatchDto
            {
                Id = m.Id,
                CreatorId = m.CreatorId,
                Title = m.Title,
                Description = m.Description,
                Date = m.Date,
                Location = m.Location,
                Sport = m.Sport.ToString(),
                Status = m.Status.ToString(),
                CreatedAt = m.CreatedAt,
                Creator = new UserDto
                {
                    Id = m.Creator.Id,
                    TeamName = m.Creator.TeamName,
                    City = m.Creator.City,
                    Logo = m.Creator.Logo,
                    Rating = m.Creator.Rating,
                    Email = m.Creator.Email,
                    Sport = m.Creator.Sport.ToString(),
                    Bio = m.Creator.Bio,
                    IsPro = m.Creator.IsPro,
                    CreatedAt = m.Creator.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(matches);
    }

    [HttpPost]
    public async Task<ActionResult<MatchDto>> CreateMatch([FromBody] CreateMatchDto dto)
    {
        var userId = GetUserId();

        if (!Enum.TryParse<SportType>(dto.Sport.ToUpper(), out var sportType))
        {
            return BadRequest(new { message = "Invalid sport type" });
        }

        var match = new Match
        {
            CreatorId = userId,
            Title = dto.Title,
            Description = dto.Description,
            Date = dto.Date,
            Location = dto.Location,
            Sport = sportType,
            Status = MatchStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Matches.Add(match);

        // Create activity
        var activity = new Activity
        {
            UserId = userId,
            Type = ActivityType.MATCH_CREATED,
            Title = "Maç İlanı Verdin",
            Description = $"{dto.Title} maçı için ilan verdin",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var creator = await _context.Users.FindAsync(userId);

        return CreatedAtAction(nameof(GetMatches), new { id = match.Id }, new MatchDto
        {
            Id = match.Id,
            CreatorId = match.CreatorId,
            Title = match.Title,
            Description = match.Description,
            Date = match.Date,
            Location = match.Location,
            Sport = match.Sport.ToString(),
            Status = match.Status.ToString(),
            CreatedAt = match.CreatedAt,
            Creator = creator != null ? new UserDto
            {
                Id = creator.Id,
                TeamName = creator.TeamName,
                City = creator.City,
                Logo = creator.Logo,
                Rating = creator.Rating,
                Email = creator.Email,
                Sport = creator.Sport.ToString(),
                Bio = creator.Bio,
                IsPro = creator.IsPro,
                CreatedAt = creator.CreatedAt
            } : null
        });
    }

    [HttpPost("{matchId}/request")]
    public async Task<IActionResult> CreateMatchRequest(string matchId)
    {
        var userId = GetUserId();

        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound(new { message = "Match not found" });

        // Check if request already exists
        var existingRequest = await _context.MatchRequests
            .FirstOrDefaultAsync(mr => mr.MatchId == matchId && mr.RequesterId == userId);

        if (existingRequest != null)
            return BadRequest(new { message = "Request already sent" });

        var request = new MatchRequest
        {
            MatchId = matchId,
            RequesterId = userId,
            Status = RequestStatus.PENDING,
            CreatedAt = DateTime.UtcNow
        };

        _context.MatchRequests.Add(request);

        // Create activity
        var activity = new Activity
        {
            UserId = userId,
            Type = ActivityType.REQUEST_SENT,
            Title = "İstek Gönderdin",
            Description = $"{match.Title} maçı için istek gönderdin",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Request sent successfully" });
    }

    // Get all match requests for current user (as receiver)
    [HttpGet("requests")]
    public async Task<IActionResult> GetMyMatchRequests()
    {
        var userId = GetUserId();

        var requests = await _context.MatchRequests
            .Include(mr => mr.Match)
            .ThenInclude(m => m.Creator)
            .Include(mr => mr.Requester)
            .Where(mr => mr.Match.CreatorId == userId)
            .Select(mr => new
            {
                mr.Id,
                mr.MatchId,
                mr.Status,
                mr.CreatedAt,
                Match = new
                {
                    mr.Match.Id,
                    mr.Match.Title,
                    mr.Match.Location,
                    mr.Match.Date,
                    mr.Match.Sport
                },
                Requester = new UserDto
                {
                    Id = mr.Requester.Id,
                    TeamName = mr.Requester.TeamName,
                    City = mr.Requester.City,
                    Logo = mr.Requester.Logo,
                    Rating = mr.Requester.Rating,
                    Email = mr.Requester.Email,
                    Sport = mr.Requester.Sport.ToString(),
                    Bio = mr.Requester.Bio,
                    IsPro = mr.Requester.IsPro,
                    CreatedAt = mr.Requester.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpGet("{matchId}/requests")]
    public async Task<IActionResult> GetMatchRequests(string matchId)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound();

        var requests = await _context.MatchRequests
            .Include(mr => mr.Requester)
            .Where(mr => mr.MatchId == matchId)
            .Select(mr => new
            {
                mr.Id,
                mr.Status,
                mr.CreatedAt,
                Requester = new UserDto
                {
                    Id = mr.Requester.Id,
                    TeamName = mr.Requester.TeamName,
                    City = mr.Requester.City,
                    Logo = mr.Requester.Logo,
                    Rating = mr.Requester.Rating,
                    Email = mr.Requester.Email,
                    Sport = mr.Requester.Sport.ToString(),
                    Bio = mr.Requester.Bio,
                    IsPro = mr.Requester.IsPro,
                    CreatedAt = mr.Requester.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(requests);
    }
}
