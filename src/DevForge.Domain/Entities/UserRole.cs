using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// Domain entity representing the user-to-role association.
/// </summary>
public class UserRole : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
