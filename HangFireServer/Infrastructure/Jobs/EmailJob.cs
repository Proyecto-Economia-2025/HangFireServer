using HangFireServer.Core.DTOs; 
using HangFireServer.Domain.Models;
using Core.Abstractions;
using HangFireServer.Core.Absttractions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class EmailJob
{
    private readonly IEventLogger _eventLogger;
    private readonly IErrorLogger _errorLogger;
    private const string EmailServerUrl = "http://localhost:5001/send-email-task";

    public EmailJob(IEventLogger eventLogger, IErrorLogger errorLogger)
    {
        _eventLogger = eventLogger;
        _errorLogger = errorLogger;
    }

    public void Send(NotificationJobRequest request)
    {

        _eventLogger.LogEvent(request.CorrelationId, "HangFireServer", "EmailJob", "EmailDispatchAttempt", new { Recipient = request.EmailAddress });

        var payloadToSend = new
        {
            request.CorrelationId,
            request.EmailAddress,
            request.Subject,
            request.MessageBody,
            request.PdfFileName 
        };

        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payloadToSend),
            Encoding.UTF8,
            "application/json");

        var client = new HttpClient();

        try
        {
            var response = client.PostAsync(EmailServerUrl, jsonContent).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                _errorLogger.LogError(null, $"Email Server respondió {response.StatusCode}.", errorDetails, request.CorrelationId);
                throw new Exception($"Fallo al enviar email. Status: {response.StatusCode}.");
            }

            _eventLogger.LogEvent(request.CorrelationId, "HangFireServer", "EmailJob", "EmailDispatchSuccess", new { Recipient = request.EmailAddress });
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException is WebException webEx &&
                (webEx.Status == WebExceptionStatus.ConnectFailure || webEx.Status == WebExceptionStatus.Timeout))
            {
                _errorLogger.LogError(null, $"NO SE PUDO CONECTAR al Email Server ({EmailServerUrl}).", ex.Message, request.CorrelationId);
                return;
            }

            _errorLogger.LogError(null, $"Error inesperado en job de Email: {ex.Message}", ex.StackTrace, request.CorrelationId);
            throw;
        }
        catch (Exception ex)
        {
            _errorLogger.LogError(null, $"Error no controlado en el job de Email: {ex.Message}", ex.StackTrace, request.CorrelationId);
            throw;
        }
    }
}