using MissAlise.Application;

namespace MissAlise.TelegramBot.Handlers
{
	internal abstract class BotAsyncHandlerBase<TData> : AsyncHandlerBase<TData>
	{
		protected readonly Bot _bot;
		internal BotAsyncHandlerBase(Bot bot)
		{
			_bot = bot;
		}

		protected virtual Task BeforeHandleAsync(TData data, CancellationToken cancel)
			=> Task.CompletedTask;
		protected virtual Task AfterHandleAsync(TData data, CancellationToken cancel)
			=> Task.CompletedTask;

		protected abstract Task HandleAsync(TData data, CancellationToken cancel);

		protected override async Task RunHandleAsync(TData data, CancellationToken cancel)
		{
			await BeforeHandleAsync(data, src.Token).ConfigureAwait(false);
			await HandleAsync(data, src.Token).ConfigureAwait(false);
			await AfterHandleAsync(data, src.Token).ConfigureAwait(false);
		}
	}
}
