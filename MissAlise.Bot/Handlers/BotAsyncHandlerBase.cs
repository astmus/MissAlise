using MissAlise.Application;
using MissAlise.Application.Interfaces;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot.Handlers
{
	internal abstract class BotAsyncHandlerBase<TData> : AsyncHandlerBase<TData>
	{
		protected readonly Bot _bot;
		protected readonly IHandleContext _ctx;
		protected Chat Chat { get; }
		protected Message Message { get; }
		protected User User { get; }

		internal BotAsyncHandlerBase(Bot bot, IHandleContext ctx)
		{
			_bot = bot;
			_ctx = ctx;
			Chat = ctx.Get<Chat>();
			User = ctx.Get<User>();
			Message = ctx.Get<Message>();
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
