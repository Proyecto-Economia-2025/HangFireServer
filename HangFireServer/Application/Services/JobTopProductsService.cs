using Core.Abstractions;
using HangFireServer.Core.Absttractions;
using HangFireServer.Domain.Models;

namespace HangFireServer.Application.Services
{
    public class JobTopProductsService : ITopProductsService
    {
        private readonly IRequestValidator _validator;
        private readonly IRequestLogger _requestLogger;
        private readonly IErrorLogger _errorLogger;
        private readonly IEventLogger _eventLogger;
        private readonly IJobService _jobService;
        private readonly IRequestEnricher _requestEnricher;

        public JobTopProductsService(
            IRequestValidator validator,
            IRequestLogger requestLogger,
            IErrorLogger errorLogger,
            IEventLogger eventLogger,
            IJobService jobService,
    IRequestEnricher requestEnricher)
        {
            _validator = validator;
            _requestLogger = requestLogger;
            _errorLogger = errorLogger;
            _eventLogger = eventLogger;
            _jobService = jobService;
            _requestEnricher = requestEnricher;
        }

        public async Task<object> JobsProcessTopProducts(TopProductsRequest request)
        {
            try
            {
              

                // Log de inicio del procesamiento
                await Task.Run(() => _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    "TopProductsProcessingStarted",
                    new { ProductCount = request.Payload.TopProducts?.Count }
                ));

                // Validar request
                var (isValid, reason, flow) = _validator.Validate(request);
                await Task.Run(() => _requestLogger.LogRequest(request, isValid, reason, flow));

                if (!isValid)
                {
                    // Log de validación fallida
                    await Task.Run(() => _eventLogger.LogEvent(
                        request.CorrelationId,
                        request.Service,
                        request.Endpoint,
                        "TopProductsValidationFailed",
                        new { Reason = reason }
                    ));

                    return new
                    {
                        status = "error",
                        reason,
                        correlationId = request.CorrelationId
                    };
                }

                // Enriquecer el request antes de encolar
                _requestEnricher.EnrichRequest(request);

                // Encolar el job para ser procesado por Hangfire
                await Task.Run(() => _jobService.EnqueueJob(request));

                // Log de job encolado exitosamente
                await Task.Run(() => _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    "TopProductsValidationJobsQueued",
                    new { }
                ));

                return new
                {
                    status = "queued",
                    correlationId = request.CorrelationId
                };
            }
            catch (Exception ex)
            {
                // Log de error con stackTrace seguro
                await Task.Run(() => _errorLogger.LogError(
                    request,
                    $"Error procesando productos más vendidos: {ex.Message}",
                    ex.StackTrace ?? string.Empty
                ));

                // Log de evento de error
                await Task.Run(() => _eventLogger.LogEvent(
                    request.CorrelationId,
                    request.Service,
                    request.Endpoint,
                    "TopProductsProcessingError",
                    new { ErrorMessage = ex.Message }
                ));

                return new
                {
                    status = "error",
                    message = "Error interno del servidor al procesar los jobs de los productos más vendidos",
                    correlationId = request.CorrelationId
                };
            }
        }
    }
}