namespace Core.Abstractions
{
    public interface IKafkaProducerService
    {
        Task<bool> ProduceAsync(string topic, string message, IDictionary<string, string> headers = null);
        Task<bool> ProduceAsync<T>(string topic, T message, IDictionary<string, string> headers = null);

        // Métodos específicos
        Task<bool> ProduceRequestLogAsync(string message, IDictionary<string, string> headers = null);
        Task<bool> ProduceErrorLogAsync(string message, IDictionary<string, string> headers = null);
        Task<bool> ProduceEventLogAsync(string message, IDictionary<string, string> headers = null);
        Task ProduceEventLogAsync(object logEntry);
    }
}