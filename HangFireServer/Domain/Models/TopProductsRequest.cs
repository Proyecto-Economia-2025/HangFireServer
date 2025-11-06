using Core.DTOs;
using HangFireServer.Core.DTOs;

namespace HangFireServer.Domain.Models
{
    public class TopProductsRequest : BaseRequest
    {
        public TopProductsPayload Payload { get; set; }
    }

    public class TopProductsPayload
    {
        public List<ProductSale> TopProducts { get; set; }
        public string requestedAction { get; set; }
        public Metadata Metadata { get; set; }
    }

    public class ProductSale
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int TotalSold { get; set; }
    }
}