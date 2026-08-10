using DevForge.Domain.Common;

namespace DevForge.Domain.Entities;

/// <summary>
/// A minimal entity used to prove database connectivity and track system status checks.
/// </summary>
public class SystemStatusInfo : Entity
{
    public string Status { get; set; } = "Ok";
    public string ApplicationName { get; set; } = "DevForge";
    public string Version { get; set; } = "1.0.0";
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}
