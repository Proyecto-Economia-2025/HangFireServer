using HangFireServer.Core.DTOs;

namespace HangFireServer.Core.Absttractions
{
    public interface IRequestValidator
    {
        (bool isValid, string reason, string flow) Validate(BaseRequest request);
    }
}