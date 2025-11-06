using HangFireServer.Core.DTOs;

namespace HangFireServer.Domain.Interfaces
{
    public interface IPdfRequestSender
    {
        /// <summary>
        /// Envía la request para generar el PDF y devuelve el resultado como string.
        /// </summary>
        /// <param name="request">Request con la información necesaria para generar el PDF</param>
        /// <returns>Resultado del endpoint PDF</returns>
        Task<string> SendPdfRequestAsync(BaseRequest request);
    }
}
