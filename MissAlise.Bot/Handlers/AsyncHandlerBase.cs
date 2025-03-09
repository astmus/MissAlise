using MissAlise.Application;

namespace MissAlise.Bot
{
	internal abstract class BotAsyncHandlerBase<TData> : AsyncHandlerBase<TData>
	{
		protected readonly Bot _bot;
		internal BotAsyncHandlerBase(Bot bot)
		{
			_bot = bot;
		}
	}
}
