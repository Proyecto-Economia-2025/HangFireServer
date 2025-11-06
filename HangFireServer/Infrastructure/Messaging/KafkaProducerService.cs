using Confluent.Kafka;
using Core.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace HangFireServer.Infrastructure.Messaging
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(
            IOptions<KafkaSettings> kafkaSettings,
            ILogger<KafkaProducerService> logger)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                MessageTimeoutMs = _kafkaSettings.MessageTimeoutMs,
                RequestTimeoutMs = _kafkaSettings.RequestTimeoutMs,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5
            };

            _producer = new ProducerBuilder<Null, string>(config)
                .SetErrorHandler((_, error) =>
                    _logger.LogError($"Error de Kafka: {error.Reason}"))
                .Build();
        }

        public async Task<bool> ProduceAsync(string topic, string message, IDictionary<string, string> headers = null)
        {
            try
            {
                var kafkaMessage = new Message<Null, string>
                {
                    Value = message,
                    Headers = new Headers()
                };

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        kafkaMessage.Headers.Add(header.Key, Encoding.UTF8.GetBytes(header.Value));
                    }
                }

                var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage);
                //_excel.guardarlog(message);
                _logger.LogInformation(
                    "Mensaje entregado a {Topic} [{Partition}] con Offset: {Offset}",
                    deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);

                return true;
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Error al enviar mensaje a Kafka: {Reason}", ex.Error.Reason);
                return false;
            }
        }

        public async Task<bool> ProduceAsync<T>(string topic, T message, IDictionary<string, string> headers = null)
        {
            var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return await ProduceAsync(topic, jsonMessage, headers);
        }

        public Task<bool> ProduceRequestLogAsync(string message, IDictionary<string, string> headers = null)
        {
            return ProduceAsync(_kafkaSettings.LogTopic, message, headers);
        }

        public Task<bool> ProduceErrorLogAsync(string message, IDictionary<string, string> headers = null)
        {
            return ProduceAsync(KafkaTopics.ErrorLogs, message, headers);
        }

        public Task<bool> ProduceEventLogAsync(string message, IDictionary<string, string> headers = null)
        {
            return ProduceAsync(KafkaTopics.EventLogs, message, headers);
        }

        public async Task ProduceEventLogAsync(object logEntry)
        {
            var jsonMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await ProduceAsync(KafkaTopics.EventLogs, jsonMessage);
        }


        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}