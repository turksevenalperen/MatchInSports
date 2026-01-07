using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MatchISportsAPI.Data;
using MatchISportsAPI.DTOs;
using MatchISportsAPI.Services;

namespace MatchISportsAPI.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public UserController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            TeamName = user.TeamName,
            City = user.City,
            Sport = user.Sport.ToString(),
            Rating = user.Rating,
            Bio = user.Bio,
            Logo = user.Logo,
            IsPro = user.IsPro,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound();

        if (!string.IsNullOrEmpty(dto.TeamName))
            user.TeamName = dto.TeamName;
        
        if (!string.IsNullOrEmpty(dto.City))
            user.City = dto.City;
        
        if (dto.Bio != null)
            user.Bio = dto.Bio;
        
        if (!string.IsNullOrEmpty(dto.Logo))
            user.Logo = dto.Logo;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            TeamName = user.TeamName,
            City = user.City,
            Sport = user.Sport.ToString(),
            Rating = user.Rating,
            Bio = user.Bio,
            Logo = user.Logo,
            IsPro = user.IsPro,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            var url = await _cloudinaryService.UploadImageAsync(file);
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? sport, [FromQuery] string? city)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(sport) && Enum.TryParse<Models.SportType>(sport.ToUpper(), out var sportType))
        {
            query = query.Where(u => u.Sport == sportType);
        }

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(u => u.City.Contains(city));
        }

        var users = await query
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

        return Ok(users);
    }

    [HttpGet("user/stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var userId = GetUserId();
        
        var matchesPlayed = await _context.MatchHistories
            .Where(mh => mh.Team1Id == userId || mh.Team2Id == userId)
            .CountAsync();

        var matchesWon = await _context.MatchHistories
            .Where(mh => (mh.Team1Id == userId && mh.Score1 > mh.Score2) || 
                        (mh.Team2Id == userId && mh.Score2 > mh.Score1))
            .CountAsync();

        var user = await _context.Users.FindAsync(userId);

        return Ok(new
        {
            matchesPlayed,
            matchesWon,
            rating = user?.Rating ?? 0,
            isPro = user?.IsPro ?? false
        });
    }

    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities()
    {
        var userId = GetUserId();
        
        var activities = await _context.Activities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .Select(a => new
            {
                a.Id,
                a.Type,
                a.Title,
                a.Description,
                a.Metadata,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(activities);
    }
}
