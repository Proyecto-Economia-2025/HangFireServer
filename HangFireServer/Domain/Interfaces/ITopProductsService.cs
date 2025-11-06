using HangFireServer.Domain.Models;

namespace HangFireServer.Core.Absttractions
{
    public interface ITopProductsService
    {
        Task<object> JobsProcessTopProducts(TopProductsRequest request);
    }
}