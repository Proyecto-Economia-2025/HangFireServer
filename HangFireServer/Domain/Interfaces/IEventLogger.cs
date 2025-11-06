using HangFireServer.Core.DTOs;

namespace Core.Abstractions
{
    public interface IEventLogger
    {
        void LogEvent(string correlationId, string service, string endpoint, string eventName, object eventData);
        void LogEvent(BaseRequest request, string eventName, object eventData);
    }
}