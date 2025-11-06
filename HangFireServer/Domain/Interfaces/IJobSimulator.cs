namespace HangFireServer.Core.Absttractions
{
    public interface IJobSimulator
    {
        /// <summary>
        /// Simula un trabajo finito con duración configurable.
        /// </summary>
        /// <param name="milliseconds">Tiempo a simular en milisegundos</param>
        Task SimulateAsync(int milliseconds);
    }
}
