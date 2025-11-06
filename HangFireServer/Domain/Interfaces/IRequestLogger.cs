using HangFireServer.Core.DTOs;

namespace Core.Abstractions
{
    public interface IRequestLogger
    {
        void LogRequest(BaseRequest request, bool isValid, string reason, string flow);
    }
}