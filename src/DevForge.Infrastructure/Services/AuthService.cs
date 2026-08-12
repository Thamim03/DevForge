using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DevForge.Application.Common.Interfaces;
using DevForge.Application.Common.Models;
using DevForge.Domain.Entities;
using DevForge.Infrastructure.Persistence;

namespace DevForge.Infrastructure.Services;

/// <summary>
/// Service implementing authentication, password hashing, and token generation.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username, email, and password are required.");
        }

        // Standard duplicate check
        var userExists = await _dbContext.Users.AnyAsync(
            u => u.Email.ToLower() == request.Email.ToLower() || u.Username.ToLower() == request.Username.ToLower(), 
            cancellationToken);

        if (userExists)
        {
            // Secure, generic error message to prevent account enumeration
            throw new InvalidOperationException("Username or email is already registered.");
        }

        // Hash password securely with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        // Get or create default "User" role
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        if (role == null)
        {
            role = new Role { Name = "User" };
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        user.UserRoles.Add(new UserRole { User = user, Role = role });

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = GenerateJwtToken(user, "User");

        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            Role = "User"
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Credentials are required.");
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(
                u => u.Email.ToLower() == request.UsernameOrEmail.ToLower() || u.Username.ToLower() == request.UsernameOrEmail.ToLower(),
                cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Verify password
        bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";
        var token = GenerateJwtToken(user, roleName);

        return new AuthResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Token = token,
            Role = roleName
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null) return null;

        var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = roleName
        };
    }

    private string GenerateJwtToken(User user, string roleName)
    {
        var secretKey = _configuration["Jwt:Key"] ?? "DefaultSecretKeyPlaceholder1234567890!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, roleName)
        };

        double expirationMinutes = 30;
        var expSetting = _configuration["Jwt:ExpirationMinutes"];
        if (expSetting != null && double.TryParse(expSetting, out var parsedMinutes))
        {
            expirationMinutes = parsedMinutes;
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "DevForgeAPI",
            audience: _configuration["Jwt:Audience"] ?? "DevForgeWeb",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
