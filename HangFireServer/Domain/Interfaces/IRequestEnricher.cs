using HangFireServer.Core.DTOs;

namespace Core.Abstractions
{
    public interface IRequestEnricher
    {
        /// <summary>
        /// Llena o actualiza la información de BaseRequest antes de encolar el job
        /// </summary>
        /// <param name="request">Request original</param>
        void EnrichRequest(BaseRequest request);
    }
}
