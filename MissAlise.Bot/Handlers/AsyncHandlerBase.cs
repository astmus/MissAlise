using MissAlise.Application;

namespace MissAlise.Bot
{
	internal abstract class BotAsyncHandlerBase<TData> : AsyncHandlerBase<TData>
	{
		protected readonly BotWorker _bot;
		internal BotAsyncHandlerBase(BotWorker bot)
		{
			_bot = bot;
		}
	}
}
