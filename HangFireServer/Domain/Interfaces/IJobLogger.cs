using HangFireServer.Core.DTOs;

namespace HangFireServer.Core.Absttractions
{
    public interface IJobLogger
    {        
        void LogJobStarted(BaseRequest request, string jobId);
        void LogJobCompleted(BaseRequest request, string jobId, object? result = null);
        void LogJobFailed(BaseRequest request, string jobId, Exception ex);
        void LogJobEvent(BaseRequest request, string jobId, string eventName, object? data = null);
    }
}
