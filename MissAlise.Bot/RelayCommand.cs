using System.CommandLine;
using MissAlise.Application.Common.RequestHandler;
using Telegram.Bot.Types;

namespace MissAlise.Bot
{
	internal class RelayCommand : Command
	{
		public ICommand CommandObject => commandAdapter(this);
		private Func<RelayCommand, ICommand> commandAdapter { get; }

		public RelayCommand(string Command, string? description, Func<RelayCommand, ICommand> commandAdapter) : base(Command, description)
		{
			this.AddAlias("/" + Name);
			this.commandAdapter = commandAdapter;
		}

		public BotCommand ToBotCommand()
		{
			return new BotCommand(Name, Description);
		}
	}
}
