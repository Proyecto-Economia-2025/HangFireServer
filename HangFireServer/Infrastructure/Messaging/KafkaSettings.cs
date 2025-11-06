namespace HangFireServer.Infrastructure.Messaging
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string LogTopic { get; set; }
        public string ConsumerGroup { get; set; }
        public int MessageTimeoutMs { get; set; } = 5000;
        public int RequestTimeoutMs { get; set; } = 3000;
        public string AutoOffsetReset { get; set; } = "Earliest";
        public bool EnableAutoCommit { get; set; } = false;
    }
}