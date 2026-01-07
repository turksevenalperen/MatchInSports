using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchISportsAPI.Data;
using MatchISportsAPI.Models;
using MatchISportsAPI.DTOs;
using MatchISportsAPI.Services;

namespace MatchISportsAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email already registered" });
        }

        // Parse sport enum
        if (!Enum.TryParse<SportType>(dto.Sport.ToUpper(), out var sportType))
        {
            return BadRequest(new { message = "Invalid sport type" });
        }

        // Create new user
        var user = new User
        {
            Email = dto.Email,
            Password = _authService.HashPassword(dto.Password),
            TeamName = dto.TeamName,
            City = dto.City,
            Sport = sportType,
            Bio = dto.Bio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create PLATFORM_JOIN activity
        var activity = new Activity
        {
            UserId = user.Id,
            Type = ActivityType.PLATFORM_JOIN,
            Title = "Platforma Katıldın",
            Description = $"{user.TeamName} takımı Match-iSports platformuna katıldı!",
            CreatedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _authService.GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            User = new UserDto
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
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !_authService.VerifyPassword(dto.Password, user.Password))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _authService.GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            User = new UserDto
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
            }
        });
    }
}
