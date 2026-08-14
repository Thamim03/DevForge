using System.Collections.Generic;

namespace DevForge.Application.Common.Models;

/// <summary>
/// Request containing raw SQL query to format.
/// </summary>
public class SqlFormatterRequest
{
    public string Sql { get; set; } = string.Empty;
}

/// <summary>
/// Response containing formatted SQL, validation status, and parsing error messages if any.
/// </summary>
public class SqlFormatterResponse
{
    public string FormattedSql { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}
