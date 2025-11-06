using HangFireServer.Core.DTOs;
using HangFireServer.Domain.Interfaces;
using System.Text;

namespace HangFireServer.Infrastructure.Validators
{
    public class CorrelationIdRule : IValidatorRule
    {
        public string ErrorMessage => "CorrelationId inválido";

        public bool Validate(BaseRequest request, StringBuilder log)
        {
            if (string.IsNullOrEmpty(request.CorrelationId))
            {
                request.CorrelationId = Guid.NewGuid().ToString();
                log.AppendLine($"CorrelationId generado automáticamente: {request.CorrelationId}");
            }
            else
            {
                log.AppendLine($"CorrelationId recibido: {request.CorrelationId}");
            }

            return true;
        }
    }
}