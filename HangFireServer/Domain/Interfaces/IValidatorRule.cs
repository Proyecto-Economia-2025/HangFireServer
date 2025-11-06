using HangFireServer.Core.DTOs;
using System.Text;

namespace HangFireServer.Domain.Interfaces
{
    public interface IValidatorRule
    {
        string ErrorMessage { get; }
        bool Validate(BaseRequest request, StringBuilder log);
    }
}