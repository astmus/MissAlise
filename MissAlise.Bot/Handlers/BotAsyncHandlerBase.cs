using MissAlise.Application;

namespace MissAlise.TelegramBot
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
		protected abstract Task RunHandleAsync(TData data, CancellationToken cancel);

		protected override async Task HandleAsync(TData data, CancellationToken cancel)
		{
			await BeforeHandleAsync(data, src.Token).ConfigureAwait(false);
			await RunHandleAsync(data, src.Token).ConfigureAwait(false);
			await AfterHandleAsync(data, src.Token).ConfigureAwait(false);
		}
	}
}
