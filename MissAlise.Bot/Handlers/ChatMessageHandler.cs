using MissAlise.Application;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Providers;
using MissAlise.Application.UseCases.Sync;
using Telegram.Bot.Types;

namespace MissAlise.Bot.Handlers
{
	internal class ChatMessageHandler : AsyncHandlerBase<Message>
	{
		private readonly IOneDriveService storage;
		private readonly IAsyncHandlersProvider provider;

		public ChatMessageHandler(IOneDriveService storage, IAsyncHandlersProvider provider)
		{
			this.storage = storage;
			this.provider = provider;
		}		

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
