using HangFireServer.Core.Absttractions;

namespace HangFireServer.Infrastructure.Jobs
{
    public class JobSimulator : IJobSimulator
    {
        public async Task SimulateAsync(int milliseconds)
        {
            if (milliseconds <= 0) return;

            await Task.Delay(milliseconds);
        }
    }
}
