
using System.CommandLine;
using System.CommandLine.Help;
using System.Text;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot;
using MissAlise.TelegramBot.Building;
using Telegram.Bot;
using Telegram.Bot.Types;

public record HelpCommand() : ICommand;

public sealed class HelpCommandHandler : ICommandHandler<HelpCommand>
{
	private readonly BotDefinition _definition;
	private readonly IHandleContext _context;
	private readonly Bot _bot;

	public HelpCommandHandler(BotDefinition definition, IHandleContext context, Bot bot)
	{
		_definition = definition;
		_context = context;
		_bot = bot;
	}

	public async Task<Result> Handle(HelpCommand request, CancellationToken cancellationToken)
	{
		var helpText = BuildHelpText(_definition.RootCommand);
		var update = _context.Get<UpdateExt>() ?? _context.Get<Update>();
		var chatId = update?.GetCurrentChat()?.Id ?? _context.Get<Chat>()?.Id ?? 0L;
		if (chatId == 0)
			return Result.Fail("Chat not found");

		await _bot.ApiClient.SendMessage(chatId, helpText, cancellationToken: cancellationToken);
		return Result.Successful;
	}

	private static string BuildHelpText(Command command)
	{
		var sb = new StringBuilder();
		using var writer = new StringWriter(sb);
		var helpBuilder = new HelpBuilder(LocalizationResources.Instance, maxWidth: 240);
		var helpContext = new HelpContext(helpBuilder, command, writer);
		helpBuilder.Write(helpContext);
		return sb.ToString();
	}
}