using Hangfire;
using Microsoft.AspNetCore.Mvc;
using HangFireServer.Domain.Models; 
using System;

[Route("api/[controller]")] 
[ApiController]
public class HangfireJobController : ControllerBase
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly EmailJob _emailJob;
    private readonly MessagingJob _messagingJob;

    public HangfireJobController(IBackgroundJobClient jobClient, EmailJob emailJob, MessagingJob messagingJob)
    {
        _jobClient = jobClient;
        _emailJob = emailJob; 
        _messagingJob = messagingJob;
    }

    // ENDPOINT PARA QUE PDF MANDE LOS REQUEST: POST /api/HangfireJob/schedule-notifications
    [HttpPost("schedule-notifications")]
    public IActionResult ScheduleNotifications([FromBody] NotificationJobRequest request)
    {
        if (request == null) return BadRequest("Datos de la tarea de notificación requeridos.");

        //Espera 45 segundos
        TimeSpan delay = TimeSpan.FromSeconds(45);
        
        
        //JOB para Email
        var emailJobId = _jobClient.Schedule(
            () => _emailJob.Send(request),
            delay
        );

        //JOB para Menssage
        var messageJobId = _jobClient.Schedule(
            () => _messagingJob.Send(request),
            delay
        );

        Console.WriteLine($"[INFO] {request.CorrelationId}: 2 Jobs de notificación programados (Email: {emailJobId}, Msg: {messageJobId}).");

        return Ok(new { status = "Jobs programados", correlationId = request.CorrelationId });
    }
}