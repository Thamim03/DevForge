using DevForge.Application.Common.Models;

namespace DevForge.Application.Common.Interfaces;

/// <summary>
/// Service contract for formatting and minifying SQL statements.
/// </summary>
public interface ISqlFormatterService
{
    SqlFormatterResponse Format(SqlFormatterRequest request);
    SqlFormatterResponse Minify(SqlFormatterRequest request);
}
