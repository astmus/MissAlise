using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal class BotCommand<TCommand> : BotCommand, ICommandParent where TCommand : class, ICommand
	{
		public TCommand SubCommand { get; set; }
		public ICommand CreateSubcommand()
			=> Activator.CreateInstance<TCommand>();
	}
}
