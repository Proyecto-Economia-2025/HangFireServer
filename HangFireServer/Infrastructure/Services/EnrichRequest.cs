using Core.Abstractions;
using HangFireServer.Core.DTOs;

namespace HangFireServer.Infrastructure.Services
{
    public class RequestEnricher : IRequestEnricher
    {
        public void EnrichRequest(BaseRequest request)
        {
            if (request == null) return;

            // Sobrescribimos el Timestamp con la hora actual
            request.Timestamp = DateTime.UtcNow;

            // Sobrescribimos el ServerHost con el nombre del servidor actual
            request.ServerHost = Environment.MachineName;

        }
    }
}
