using Core.Abstractions;
using Hangfire;
using HangFireServer.Configuration;
using HangFireServer.Core.Absttractions;
using HangFireServer.Domain.Interfaces;
using HangFireServer.Infrastructure.Logging;
using HangFireServer.Infrastructure.Messaging;
using HangFireServer.Infrastructure.PDFs;
using Infrastructure.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using DiscordSettings = HangFireServer.Configuration.DiscordSettings;

var builder = WebApplication.CreateBuilder(args);


// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Kestrel para usar HTTP en puerto específico
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7192); // HTTP
});

// -----------------------------
// Configuración de servicios existente
// -----------------------------
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IEventLogger, EventLogger>();
builder.Services.AddSingleton<IErrorLogger, ErrorLogger>();

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddHttpClient<IPdfRequestSender, PdfRequestSender>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"));
    config.UseRecommendedSerializerSettings();
});

builder.Services.Configure<DiscordSettings>(
    builder.Configuration.GetSection("Discord"));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HangFire Server API",
        Version = "v1",
        Description = "API para gestión de trabajos con HangFire"
    });
});

var app = builder.Build();

// Usar CORS ANTES de otros middlewares
app.UseCors("AllowAll");

// Limpiar jobs huérfanos
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.RemoveIfExists("job_recurrente_minuto");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HangFire Server API V1");
    });
}


app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard("/swagger/hangfire");

// Publicar URL en Discord
try
{
    //abrimos el tunnel dinamicamente por aquello que se use 

    // Obtener la URL din�mica de Cloudflare (espera y deja cloudflared corriendo)
    var (cloudUrl, logPath, pid) = await HangFireServer.Configuration.NetworkHelper
        .GetCloudflareTunnelUrlAsync(
            timeoutMs: 120000,    // 120 s
            localPort: 7192,
            leaveRunning: true,
            statusIntervalMs: 5000);


    Console.WriteLine($"[INFO] cloudflared log: {logPath}");
    Console.WriteLine($"[INFO] cloudflared PID: {(pid.HasValue ? pid.Value.ToString() : "no-running")}");

    string hangUrl = "https://hangserver.escritorio.tonyml.com";
    //la u me bloquea la red asi que ha usar la local
    hangUrl = "https://localhost:7192";
    Console.WriteLine($"[INFO] Publicando URL de Hang: {hangUrl}");

    string discordMessage = $"🚀 **HangFire Server Iniciado**\n" +
                           $"📅 **Fecha**: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                           $"🔗 **URL**: {hangUrl}\n" +
                           $"📊 **Dashboard**: {hangUrl}/hangfire\n" +
                           $"{{  \"cloudflare\": \"{cloudUrl}\" }}";

    var discordSettings = app.Services.GetRequiredService<IOptions<DiscordSettings>>().Value;
    var publisher = new HangFireServer.Configuration.DiscordPublisher(
        discordSettings.Token,
        discordSettings.ChannelId
    );

    await publisher.PublishIpAsync(discordMessage);
    Console.WriteLine($"[INFO] URL de Hang publicada en Discord exitosamente");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] No se pudo publicar la URL en Discord: {ex.Message}");
}


app.Run();