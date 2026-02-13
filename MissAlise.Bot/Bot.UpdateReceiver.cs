using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	internal partial class Bot<TUpdate> where TUpdate : Update
	{
		internal sealed class UpdateReceiver : BackgroundService
		{
			private int _messageOffset;
			private readonly ILogger<UpdateReceiver> _log;
			private Bot<TUpdate>? _bot;

			public UpdateReceiver(ILogger<UpdateReceiver> log, Bot<TUpdate> bot)
			{
				_log = log;
				_bot = bot;
			}

			private async Task InitializeAsync(CancellationToken cancel)
			{
				_bot.Info = await _bot.ApiClient.GetMe(cancel);

				await _bot.ApiClient.DeleteMyCommands(cancellationToken: cancel);

				_log.LogInformation("Connected as {Bot}", _bot.Info);
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				await InitializeAsync(cancel);
				await DropPendingUpdatesIfNeeded(cancel).ConfigureAwait(false);

				var getUpdatesRequest = new GetUpdatesRequest<TUpdate>
				{
					Offset = _messageOffset,
					Limit = _bot.receiveOptions.Limit,
					Timeout = 120,
					AllowedUpdates = _bot.receiveOptions.AllowedUpdates,
				};

				var backoffMs = 200;
				while (!cancel.IsCancellationRequested)
				{
					try
					{
						var updateArray = await _bot.ApiClient.SendRequest(getUpdatesRequest, cancellationToken: cancel).ConfigureAwait(false);

						if (updateArray.Length > 0)
						{
							_messageOffset = updateArray[^1].Id + 1;

							foreach (var update in updateArray)
							{
								await _bot.Updates.Writer.WriteAsync(update);
							}

							getUpdatesRequest.Offset = _messageOffset;
						}

						backoffMs = 200;
					}
					catch (OperationCanceledException)
					{
						return;
					}
					catch (Telegram.Bot.Exceptions.RequestException ex) when (ex.InnerException is TaskCanceledException or TimeoutException)
					{
						await Task.Delay(backoffMs, cancel).ConfigureAwait(false);
						backoffMs = Math.Min(backoffMs * 2, 5000);
						continue;
					}
					catch (Exception ex)
					{
						await _bot.HandleErrorAsync(ex, cancel).ConfigureAwait(false);
						await Task.Delay(backoffMs, cancel).ConfigureAwait(false);
						backoffMs = Math.Min(backoffMs * 2, 10000);
					}
				}

				_bot.Updates.Writer.Complete();
			}

			private async Task DropPendingUpdatesIfNeeded(CancellationToken cancel)
			{
				if (_bot.receiveOptions.DropPendingUpdates is false)
					return;

				try
				{
					var updates = await _bot.ApiClient.GetUpdates(-1, 1, 0, [], cancel).ConfigureAwait(false);
					_messageOffset = updates.Length == 0 ? 0 : updates[^1].Id + 1;
				}
				catch (OperationCanceledException)
				{				
				}
			}
		}
	}
}
