using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs;
using Hangfire.Server;
using HangFireServer.Domain.Interfaces;

namespace HangFireServer.Infrastructure.Hangfire
{
    public class HangFireJobProcessor : IJobProcessor
    {
        private readonly IJobLogger _jobLogger;
        private readonly IJobSimulator _jobSimulator;
        private readonly IPdfRequestSender _pdfRequestSender;

        public HangFireJobProcessor(
            IJobLogger jobLogger,
            IJobSimulator jobSimulator,
            IPdfRequestSender pdfRequestSender)
        {
            _jobLogger = jobLogger;
            _jobSimulator = jobSimulator;
            _pdfRequestSender = pdfRequestSender;
        }

        public async Task ProcessAsync(BaseRequest request, PerformContext? context)
        {
            var jobId = context?.BackgroundJob?.Id ?? Guid.NewGuid().ToString();

            try
            {
                // Log de inicio de job
                _jobLogger.LogJobStarted(request, jobId);

                Console.WriteLine($"Procesando job {jobId} con CorrelationId: {request.CorrelationId}");

                // Llamada al endpoint PDF
                var result = await _pdfRequestSender.SendPdfRequestAsync(request);

               
                // Log de finalización
                _jobLogger.LogJobCompleted(request, jobId, result);
            }
            catch (Exception ex)
            {
                // Log de error
                _jobLogger.LogJobFailed(request, jobId, ex);
                throw; // re-lanzar para que Hangfire maneje retries
            }
        }
    }
}
