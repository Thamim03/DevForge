using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing a role in the system (e.g. Admin, User).
/// </summary>
public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation property for EF Core
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
