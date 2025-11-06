namespace HangFireServer.Domain.Models
{
    public class NotificationJobRequest
    {
        public string CorrelationId { get; set; }
        public string PdfFileName { get; set; }

        public string EmailAddress { get; set; }
        public string MessageRecipient { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public string PlatformType { get; set; }
    }
}