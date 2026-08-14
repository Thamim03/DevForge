using System.Threading;
using System.Threading.Tasks;
using DevForge.Application.Common.Models;

namespace DevForge.Application.Common.Interfaces;

/// <summary>
/// Service contract handling secure outbound HTTP requests for the API Playground.
/// </summary>
public interface IApiPlaygroundService
{
    Task<ApiPlaygroundResponse> SendRequestAsync(ApiPlaygroundRequest request, CancellationToken cancellationToken = default);
}
