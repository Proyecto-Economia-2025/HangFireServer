namespace HangFireServer.Presentation.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Obtener el correlation ID del header o generar uno nuevo
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            var isNewCorrelationId = string.IsNullOrEmpty(correlationId);

            if (isNewCorrelationId)
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Append("X-Correlation-ID", correlationId);
            }

            // Agregar el correlation ID a los headers de respuesta
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Correlation-ID", correlationId);
                return Task.CompletedTask;
            });

            // Loggear el inicio del request
            _logger.LogInformation(
                "Iniciando request {CorrelationId} - Método: {Method}, Ruta: {Path}, Origen: {Origin}",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                isNewCorrelationId ? "Generado" : "Recibido");

            // Medir el tiempo de procesamiento
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Continuar con el pipeline
                await _next(context);

                stopwatch.Stop();

                // Loggear la finalización exitosa
                _logger.LogInformation(
                    "Request completado {CorrelationId} - Status: {StatusCode}, Duración: {ElapsedMs}ms",
                    correlationId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Loggear el error
                _logger.LogError(
                    ex,
                    "Error procesando request {CorrelationId} - Status: {StatusCode}, Duración: {ElapsedMs}ms",
                    correlationId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                // Relanzar la excepción para que otros middlewares la manejen
                throw;
            }
        }
    }
}