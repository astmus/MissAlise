using System.Threading.Channels;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal partial class Bot
	{
		public class UpdatesChannel
		{
			public UpdatesChannel(HttpClient client)
			{
				Client = client;
			}

			public ChannelReader<Update> Pending => _channel.Reader;
			public ChannelWriter<Update> Incoming => _channel.Writer;
			public HttpClient Client { get; }

			readonly Channel<Update> _channel = Channel.CreateUnbounded<Update>(
				new()
				{
					SingleReader = true,
					SingleWriter = true,
				}
			);
		}
	}
}
