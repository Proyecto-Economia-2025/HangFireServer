using Core.Abstractions;
using HangFireServer.Application.Services;
using HangFireServer.Core.Absttractions;
using HangFireServer.Domain.Interfaces;
using HangFireServer.Infrastructure.Hangfire;
using HangFireServer.Infrastructure.Jobs;
using HangFireServer.Infrastructure.Logging;
using HangFireServer.Infrastructure.Messaging;
using HangFireServer.Infrastructure.PDFs;
using HangFireServer.Infrastructure.Services;
using HangFireServer.Infrastructure.Validators;
using Infrastructure.Logging;
using Microsoft.Win32;

namespace HangFireServer.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuración de Kafka ya se hizo en Program.cs, solo registrar servicios
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

            // Validators
            services.AddScoped<IRequestValidator, CoreRequestValidator>();
            services.AddScoped<IValidatorRule, CorrelationIdRule>();
            services.AddScoped<IValidatorRule, RequiredFieldsRule>();

            // Services
            services.AddScoped<ITopProductsService, JobTopProductsService>();

            // Enriquecedor de requests
            services.AddScoped<IRequestEnricher, RequestEnricher>();

            services.AddScoped<EmailJob>();
            services.AddScoped<MessagingJob>();

            // Jobs
            services.AddScoped<IJobService, HangFireJobService>();
            services.AddScoped<IJobProcessor, HangFireJobProcessor>();
            services.AddScoped<IPdfRequestSender, PdfRequestSender>();
            services.AddHttpClient<IPdfRequestSender, PdfRequestSender>(); // Para HttpClient inyectado
            services.AddScoped<IJobLogger, JobLogger>();
            services.AddScoped<IJobSimulator, JobSimulator>();


            // Loggers
            services.AddScoped<IRequestLogger, RequestLogger>();
            services.AddScoped<IErrorLogger, ErrorLogger>();
            services.AddScoped<IEventLogger, EventLogger>();

            return services;
        }
    }
}