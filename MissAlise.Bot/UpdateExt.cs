using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal class UpdateExt : Update
	{ 
		public ICommand Command { get; set; }
	}
}
