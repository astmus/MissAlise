using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal class AuthorizationCompleteHandler : IAuthorizationCompleter
	{
		private readonly Bot bot;
		private readonly IUserProfilesRepository repository;

		public AuthorizationCompleteHandler(Bot bot, IUserProfilesRepository repository)
		{
			this.bot = bot;
			this.repository = repository;
		}

		public async Task AuthorizationCompleted(string state, Credentials credentials, CancellationToken cancel)
		{
			var profile = await  repository.FindAsync(state, default);
			profile.AccessData = credentials;
			await repository.AddOrReplaceAsync(profile, default);
			var chatId = int.Parse(state);
			//await bot.Client.SetChatMenuButton(chatId, new MenuButtonCommands() {  }, default);
			
			await bot.Client.SendMessage(chatId, "Авторизация успешна", parseMode: ParseMode.MarkdownV2, cancellationToken: default);
			await bot.Client.SetMyCommands(bot.Commands, BotCommandScope.Chat(chatId), cancellationToken: cancel);
		}
	}
}
