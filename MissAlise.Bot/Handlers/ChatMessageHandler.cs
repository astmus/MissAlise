using Microsoft.Extensions.Logging;
using MissAlise.Application;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Providers;
using MissAlise.Application.UseCases.Sync;
using MissAlise.Interfaces;

namespace MissAlise.Bot.Handlers
{
	internal class ChatMessageHandler : AsyncHandlerBase<Telegram.Bot.Types.Message>
	{
		private readonly IOneDriveService storage;
		private readonly IAsyncHandlersProvider provider;
		private readonly ILogger<ChatMessageHandler> log;
		private readonly IUserProfilesRepository userProfiles;

		public ChatMessageHandler(IOneDriveService storage, IAsyncHandlersProvider provider, ILogger<ChatMessageHandler> log, IUserProfilesRepository userProfiles)
		{
			this.storage = storage;
			this.provider = provider;
			this.log = log;
			this.userProfiles = userProfiles;
		}

		protected override async Task HandleAsync(Telegram.Bot.Types.Message data, CancellationToken cancel)
		{
			//var list =  storage.Root.Query<DriveItem>().Select(s=> s.Photo).ToList();
			if (data.Text == "/sync")
			{
				var handler = provider.GetHandler<SyncCommand>();
				await handler.InvokeAsync(new SyncCommand(), cancel);
			}
			//var infoIt = await storage.GetRootItems(cancel);
			var info = await storage.GetOwnerInfo(cancel);
		}
	}
}
