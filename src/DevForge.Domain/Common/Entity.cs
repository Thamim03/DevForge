namespace DevForge.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
