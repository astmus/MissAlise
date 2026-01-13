using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	internal class UpdateExt : Update
	{ 
		public ICommand Command { get; set; }
	}
}
