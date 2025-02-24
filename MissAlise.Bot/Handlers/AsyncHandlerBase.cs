using MissAlise.Application;
using MissAlise.Application.Interfaces;
using Telegram.Bot;

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
