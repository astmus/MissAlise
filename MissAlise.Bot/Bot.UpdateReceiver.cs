using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	internal partial class Bot<TUpdate> where TUpdate: Update
	{
		internal sealed class UpdateReceiver : BackgroundService
		{
			private readonly ILogger<UpdateReceiver> _log;
			private readonly IServiceScopeFactory _scopeFactory;

			private IServiceScope? _scope;
			private Bot<TUpdate>? _bot;

			public UpdateReceiver(ILogger<UpdateReceiver> log, IServiceScopeFactory scopeFactory)
			{
				_log = log;
				_scopeFactory = scopeFactory;
			}

			private async Task InitializeAsync(CancellationToken ct)
			{
				_scope = _scopeFactory.CreateScope();

				try
				{
					_bot = _scope.ServiceProvider.GetRequiredService<Bot<TUpdate>>();
					_bot.Information = await _bot.ApiClient.GetMe(ct);
					await _bot.ApiClient.DeleteMyCommands(cancellationToken: ct);

					_log.LogInformation("Connected as {Bot}", _bot.Information);
				}
				catch
				{
					_scope.Dispose();
					_scope = null;
					_bot = null;
					throw;
				}
			}

			protected override async Task ExecuteAsync(CancellationToken ct)
			{
				await InitializeAsync(ct);

				try
				{
					if (_bot is null) return;

					await foreach (var update in _bot.UpdatesSource.WithCancellation(ct))
					{
						_log.LogInformation("Got update {Id}", update.Id);
						await _bot.Updates.Writer.WriteAsync(update, ct);
					}
				}
				catch (OperationCanceledException) when (ct.IsCancellationRequested)
				{
					// normal stop
				}
				catch (Exception ex)
				{
					_log.LogError(ex, "UpdateReceiver crashed");
					throw; // опционально: чтобы хост остановился/перезапустился
				}
			}

			public override async Task StopAsync(CancellationToken cancellationToken)
			{
				await base.StopAsync(cancellationToken);

				_scope?.Dispose();
				_scope = null;
				_bot = null;
			}
		}

	}
}
