using Core.Abstractions;
using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs;

namespace HangFireServer.Infrastructure.Jobs
{
    public class JobLogger : IJobLogger
    {
        private readonly IEventLogger _eventLogger;
        private readonly IErrorLogger _errorLogger;

        public JobLogger(IEventLogger eventLogger, IErrorLogger errorLogger)
        {
            _eventLogger = eventLogger;
            _errorLogger = errorLogger;
        }

        public void LogJobStarted(BaseRequest request, string jobId)
        {
            _eventLogger.LogEvent(request, "JobStarted", new { JobId = jobId });
        }

        public void LogJobCompleted(BaseRequest request, string jobId, object? result = null)
        {
            _eventLogger.LogEvent(request, "JobCompleted", new { JobId = jobId, Result = result });
        }

        public void LogJobFailed(BaseRequest request, string jobId, Exception ex)
        {
            _errorLogger.LogError(request, ex.Message, ex.StackTrace ?? string.Empty);
            _eventLogger.LogEvent(request, "JobFailed", new { JobId = jobId, ErrorMessage = ex.Message });
        }

        public void LogJobEvent(BaseRequest request, string jobId, string eventName, object? data = null)
        {
            _eventLogger.LogEvent(request, eventName, new { JobId = jobId, Data = data });
        }
    }
}
