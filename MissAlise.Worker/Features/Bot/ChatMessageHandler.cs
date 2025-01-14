using MissAlise.Interfaces;
using Telegram.Bot.Types;

namespace MissAlise.Worker.Features.Bot
{
	public class ChatMessageHandler : AsyncHandlerBase<Message>
	{
		private readonly IRemoteStorageService storage;

		public ChatMessageHandler(IRemoteStorageService storage)
		{
			this.storage = storage;
		}
		protected override Task OnErroAsync(Exception error) 
			=> throw new NotImplementedException();
		protected override async Task HandleAsync(Message data, CancellationToken cancel)
		{
			var info = await storage.GetOwnerInfo(cancel);
		}
	}
}
