namespace HangFireServer.Core.DTOs
{
    public class BaseRequest
    {
        public string CorrelationId { get; set; }
        public string Service { get; set; }
        public string Endpoint { get; set; }
        public DateTime Timestamp { get; set; }
        public bool? Success { get; set; }
        public int ExecutionTimeMs { get; set; }
        public string ServerHost { get; set; }
        public string EmailAddress { get; set; }
        public string MessageRecipient { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public string PlatformType { get; set; }
    }
}
