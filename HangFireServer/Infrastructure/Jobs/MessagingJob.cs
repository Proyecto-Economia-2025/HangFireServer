using Core.Abstractions;
using HangFireServer.Core.Absttractions;
using HangFireServer.Domain.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class MessagingJob
{
    private readonly IEventLogger _eventLogger;
    private readonly IErrorLogger _errorLogger;
    private const string MessagingServerUrl = "http://localhost:3000/api/pdf/process-message"; 

    public MessagingJob(IEventLogger eventLogger, IErrorLogger errorLogger)
    {
        _eventLogger = eventLogger;
        _errorLogger = errorLogger;
    }

    public void Send(NotificationJobRequest request)
    {
        _eventLogger.LogEvent(request.CorrelationId, "HangFireServer", "MessagingJob", "MessagingDispatchAttempt", new { Recipient = request.MessageRecipient, Platform = request.PlatformType });

        var payloadToSend = new
        {
            request.CorrelationId,
            RecipientId = request.MessageRecipient,
            Platform = request.PlatformType,
            Message = request.MessageBody,
            PdfFileName = request.PdfFileName
        };

        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payloadToSend),
            Encoding.UTF8,
            "application/json");

        var client = new HttpClient();

        try
        {
            var response = client.PostAsync(MessagingServerUrl, jsonContent).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                _errorLogger.LogError(null, $"Messages Server respondió {response.StatusCode} para {request.CorrelationId}.", errorDetails, request.CorrelationId);

                throw new Exception($"Fallo al enviar mensaje. Status: {response.StatusCode}.");
            }

            _eventLogger.LogEvent(request.CorrelationId, "HangFireServer", "MessagingJob", "MessagingDispatchSuccess", new { Recipient = request.MessageRecipient });
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException is WebException webEx &&
                (webEx.Status == WebExceptionStatus.ConnectFailure || webEx.Status == WebExceptionStatus.Timeout))
            {
                _errorLogger.LogError(null, $"NO SE PUDO CONECTAR al Messages Server. Job finaliza como Succeeded (warning).", ex.Message, request.CorrelationId);
                _eventLogger.LogEvent(request.CorrelationId, "HangFireServer", "MessagingJob", "DispatchWarning_ConnectionFailure", new { TargetUrl = MessagingServerUrl });
                return; 
            }

            _errorLogger.LogError(null, $"Error inesperado en la comunicación HTTP para {request.CorrelationId}: {ex.Message}", ex.StackTrace, request.CorrelationId);
            throw;
        }
        catch (Exception ex)
        {
            _errorLogger.LogError(null, $"Error no controlado en el job de Mensajería: {ex.Message}", ex.StackTrace, request.CorrelationId);
            throw;
        }
    }
}