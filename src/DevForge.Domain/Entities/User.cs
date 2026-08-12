using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing a user in the system.
/// </summary>
public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation property for EF Core
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
