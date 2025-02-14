using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal class AuthorizationCompleteHandler : IAuthorizationCompleter
	{
		private readonly BotWorker bot;
		private readonly IUserProfilesRepository repository;
		private readonly UserManager<AppUser> manager;
		private readonly ILogger<AuthorizationCompleteHandler> logger;

		public AuthorizationCompleteHandler(BotWorker bot, IUserProfilesRepository repository, UserManager<AppUser> manager, ILogger<AuthorizationCompleteHandler> logger)
		{
			this.bot = bot;
			this.repository = repository;
			this.manager = manager;
			this.logger = logger;
		}

		public async Task AuthorizationCompleted(string state, UserCredentials credentials, CancellationToken cancel)
		{
			try
			{
				var profile = await repository.FindAsync(state, cancel);
				profile ??= new UserProfile()
				{
				 Telegram = new User() { Id = state }
				};
				profile.AccessData = credentials;
				//profile.Id = state;
				await repository.AddOrReplaceAsync(profile, cancel);
				//var chatId = long.Parse(state);
				var tmpUser = await repository.PopPendingUser(state, cancel);

				var user = new AppUser
				{
					Id = tmpUser?.Id ?? state,
					UserName = tmpUser?.DisplayName ?? state
				};

				var result = await manager.CreateAsync(user);			

				await bot.Client.SendMessage(long.Parse(state), "Авторизация успешна", parseMode: ParseMode.MarkdownV2, cancellationToken: default);
			}
			catch (Exception error)
			{
				logger.LogError(error,error.Message);
				throw;
			}
			//await bot.Client.SetMyCommands(bot.Commands, BotCommandScope.Chat(chatId), cancellationToken: cancel);
		}
	}
}
