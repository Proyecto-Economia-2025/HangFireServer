using HangFireServer.Core.DTOs;

namespace HangFireServer.Core.Absttractions
{
    public interface IErrorLogger
    {
        Task LogError(string correlationId, string service, string endpoint, string errorMessage, string? stackTrace = null);
        Task LogError(BaseRequest request, string errorMessage, string? stackTrace = null);
    }
}
