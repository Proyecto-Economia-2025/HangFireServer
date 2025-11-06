using HangFireServer.Core.DTOs;
using System.Linq.Expressions;

namespace HangFireServer.Core.Absttractions
{
    public interface IJobService
    {
        void EnqueueJob(BaseRequest request);
    }
}
