using Core.Abstractions;
using Hangfire;
using Hangfire.Server;
using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs;

namespace HangFireServer.Infrastructure.Hangfire
{
    public class HangFireJobService : IJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IJobProcessor _jobProcessor;
        private readonly IEventLogger _eventLogger;
        private readonly IErrorLogger _errorLogger;

        public HangFireJobService(IBackgroundJobClient backgroundJobClient, IJobProcessor jobProcessor, IEventLogger eventLogger, IErrorLogger errorLogger)
        {
            _backgroundJobClient = backgroundJobClient;
            _jobProcessor = jobProcessor;
            _eventLogger = eventLogger;
            _errorLogger = errorLogger;
        }

        public void EnqueueJob(BaseRequest request)
        {
            int seconds = 10; // Delay de 10 segundos

            // Log de job encolado
            _eventLogger.LogEvent(request, "JobEnqueued", new { ScheduledInSeconds = seconds });

            // Encola el job con un delay de 10 segundos
            _backgroundJobClient.Schedule<HangFireJobService>(
                x => x.ProcessTopProductsJob(request, null),
                TimeSpan.FromSeconds(seconds)
            );
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessTopProductsJob(BaseRequest request, PerformContext? context)
        {
            try
            {
                await _jobProcessor.ProcessAsync(request, context);
                _eventLogger.LogEvent(request, "JobProcessed", new { Success = true });
            }
            catch (Exception ex)
            {
                _errorLogger.LogError(request, ex.Message, ex.StackTrace);
                _eventLogger.LogEvent(request, "JobProcessed", new { Success = false, Error = ex.Message });
                throw;
            }
        }
    }
}
