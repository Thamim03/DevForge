using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence;

/// <summary>
/// Handles database migration and seeding operations on application startup.
/// </summary>
public class DbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbContextInitializer> _logger;

    public DbContextInitializer(ApplicationDbContext context, ILogger<DbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlite())
            {
                // In-memory sqlite requires EnsureCreated rather than Migrate
                await _context.Database.EnsureCreatedAsync();
            }
            else if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Roles
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role { Name = "Admin" };
            _context.Roles.Add(adminRole);
        }

        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new Role { Name = "User" };
            _context.Roles.Add(userRole);
        }

        await _context.SaveChangesAsync();

        // 2. Seed Admin User
        var adminEmail = "admin@devforge.com";
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await _context.SaveChangesAsync();
        }

        // 3. Seed Normal User
        var userEmail = "user@devforge.com";
        var normalUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (normalUser == null)
        {
            normalUser = new User
            {
                Username = "user",
                Email = userEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!")
            };
            _context.Users.Add(normalUser);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = normalUser.Id, RoleId = userRole.Id });
            await _context.SaveChangesAsync();
        }
    }
}
