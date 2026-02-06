using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	public class UpdateExt : Update
	{ 
		public ICommand Command { get; set; }
	}
}
