using MissAlise.Interfaces;
using MissAlise.Worker.Features.Sync;
using MissAlise.Worker.Providers;
using Telegram.Bot.Types;

namespace MissAlise.Worker.Features.Bot
{
	public class ChatMessageHandler : AsyncHandlerBase<Message>
	{
		private readonly IRemoteStorageService storage;
		private readonly IAsyncHandlersProvider provider;

		public ChatMessageHandler(IRemoteStorageService storage, IAsyncHandlersProvider provider)
		{
			this.storage = storage;
			this.provider = provider;
		}
		protected override Task OnErroAsync(Exception error) 
			=> throw new NotImplementedException();
		protected override async Task HandleAsync(Message data, CancellationToken cancel)
		{
			if (data.Text == "/sync")
			{
				var handler = provider.GetHandler<SyncCommand>();
				await handler.InvokeAsync(new SyncCommand(), cancel);
			}
			var info = await storage.GetOwnerInfo(cancel);
		}
	}
}
