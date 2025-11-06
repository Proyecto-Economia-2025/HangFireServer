using Core.Abstractions;
using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs;
using System.Text.Json;

namespace HangFireServer.Infrastructure.Logging
{
    public class ErrorLogger : IErrorLogger
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly string _errorTopic;

        public ErrorLogger(IKafkaProducerService kafkaProducer, IConfiguration configuration)
        {
            _kafkaProducer = kafkaProducer;
            _errorTopic = configuration["Kafka:ErrorTopic"] ?? KafkaTopics.ErrorLogs;
        }

        public async Task LogError(string correlationId, string service, string endpoint, string errorMessage, string? stackTrace = null)
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                Service = service,
                Endpoint = endpoint,
                ErrorMessage = errorMessage,
                StackTrace = stackTrace,
                Type = "Error"
            };

            try
            {
                var jsonMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var headers = new Dictionary<string, string>
                {
                    { "CorrelationId", correlationId },
                    { "LogLevel", "Error" },
                    { "Source", service }
                };

                await _kafkaProducer.ProduceErrorLogAsync(jsonMessage, headers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando log de error a Kafka: {ex.Message}");
                Console.WriteLine($"[{DateTime.UtcNow}] CorrelationId: {correlationId} | Error: {errorMessage}");
            }
        }

        // Sobrecarga para BaseRequest
        public Task LogError(BaseRequest request, string errorMessage, string? stackTrace = null)
        {
            return LogError(request.CorrelationId, request.Service, request.Endpoint, errorMessage, stackTrace);
        }

    }
}