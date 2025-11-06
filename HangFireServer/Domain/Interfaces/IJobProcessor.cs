using Hangfire.Server;
using HangFireServer.Core.DTOs;

namespace HangFireServer.Core.Absttractions
{
    public interface IJobProcessor
    {
        Task ProcessAsync(BaseRequest request, PerformContext? context);
    }
}
