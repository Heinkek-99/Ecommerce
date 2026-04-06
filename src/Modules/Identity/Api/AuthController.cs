using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Identity.Application.Commands.RegisterUser;
using Ecommerce.Identity.Infrastructure;
using Ecommerce.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Wolverine;

namespace Ecommerce.Identity.Api;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly IdentityDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(IMessageBus bus, IdentityDbContext db, IConfiguration config)
    {
        _bus = bus;
        _db = db;
        _config = config;
    }

    [HttpPost("register")]
    [AllowAnonymous]
<<<<<<< HEAD
<<<<<<< HEAD
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        // Rôle toujours forcé à "buyer" — le champ role du body est ignoré
        var cmd = new RegisterUserCommand(req.Email, req.Password, req.FullName, req.Phone, "buyer");
=======
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd)
    {
>>>>>>> feature/orders-logic
=======
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd)
    {
>>>>>>> feature/notifications-logic
        var result = await _bus.InvokeAsync<Result<Guid>>(cmd);
        return result.IsSuccess ? Ok(new { userId = result.Value }) : BadRequest(result.Error);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant());

        if (user is null) return Unauthorized("Invalid credentials.");

        var hasher = new PasswordHasher<Domain.User>();
        var result = hasher.VerifyHashedPassword(null!, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed) return Unauthorized("Invalid credentials.");

<<<<<<< HEAD
<<<<<<< HEAD
        var jwtKey = _config["JwtSettings:SecretKey"] ?? _config["JWT_SECRET"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        {
            return StatusCode(500, "JWT Secret key is missing or too short (min 32 chars).");
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
        };

        // Multi-rôles : un claim par rôle (buyer et/ou seller)
        foreach (var role in user.GetRoles())
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            roles = user.GetRoles()
        });
    }
}

public record RegisterRequest(string Email, string Password, string FullName, string? Phone = null);
=======
=======
>>>>>>> feature/notifications-logic
        var jwtKey = _config["Jwt:Key"] ?? "ecommerce-super-secret-key-32chars!!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
            ],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

<<<<<<< HEAD
>>>>>>> feature/orders-logic
=======
>>>>>>> feature/notifications-logic
public record LoginRequest(string Email, string Password);
