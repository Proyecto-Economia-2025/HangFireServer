using HangFireServer.Core.Absttractions;
using HangFireServer.Core.DTOs;
using HangFireServer.Domain.Interfaces;
using System.Text;

namespace HangFireServer.Infrastructure.Validators
{
    public class CoreRequestValidator : IRequestValidator
    {
        private readonly IEnumerable<IValidatorRule> _rules;

        public CoreRequestValidator(IEnumerable<IValidatorRule> rules)
        {
            _rules = rules;
        }

        public (bool isValid, string reason, string flow) Validate(BaseRequest request)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine("=== Inicio de validación ===");

            foreach (var rule in _rules)
            {
                if (!rule.Validate(request, logBuilder))
                    return (false, rule.ErrorMessage, logBuilder.ToString());
            }

            logBuilder.AppendLine("=== Validación completada con éxito ===");
            return (true, "Solicitud válida", logBuilder.ToString());
        }
    }
}