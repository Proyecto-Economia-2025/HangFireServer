using Discord;
using Discord.WebSocket;

namespace HangFireServer.Configuration
{
    public class DiscordPublisher
    {
        private readonly string _token;
        private readonly ulong _channelId;
        private readonly DiscordSocketClient _client;

        public DiscordPublisher(string token, ulong channelId)
        {
            _token = token;
            _channelId = channelId;
            _client = new DiscordSocketClient();
        }

        public async Task PublishIpAsync(string ipPort)
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            // Esperar a que el cliente esté listo
            await Task.Delay(2000);

            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync(ipPort);
            }

            await _client.LogoutAsync();
        }
    }
}
