using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Identity;
using MissAlise.Interfaces;
using MissAlise.ValueObjects.Identity;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot.Commands
{
	public record StartCommand() : ICommand;
	public class StartCommandHandler : ICommandHandler<StartCommand>
	{
		private readonly IHandleContext _ctx;
		private readonly IUserProfilesRepository _userProfiles;
		private readonly IOneDriveService _oneService;
		private readonly IOwnerResolver _ownerResolver;

		public StartCommandHandler(IHandleContext ctx, IUserProfilesRepository authService, IOneDriveService oneService, IOwnerResolver ownerResolver)
		{
			_ctx = ctx;
			_userProfiles = authService;
			_oneService = oneService;
			_ownerResolver = ownerResolver;
		}

		public async Task<Result> Handle(StartCommand request, CancellationToken cancellationToken)
		{
			var tgUser = _ctx.GetCurrent<User>();
			var _update = _ctx.Get<UpdateExt>();
			var identity = new ExternalIdentity("telegram", tgUser.Id.ToString());
			var bot = _ctx.Get<Bot>("bot");
			var userId = await _ownerResolver.ResolveOwnerIdAsync(identity, cancellationToken);

			var profile = await _userProfiles.FindByOwnerIdAsync(userId, cancellationToken);
			if (profile is null)
			{
				var newProfile = new UserProfile(userId, [identity]);
				await _userProfiles.SaveAsync(newProfile, cancellationToken).ConfigureAwait(false);
			}

			var link = _oneService.CreateAuthorizeLink(_update.Message.Chat.Id);
			await bot.ApiClient.SetChatMenuButton(_update.Message.Chat.Id, new MenuButtonWebApp() { Text = "Авторизация", WebApp = new WebAppInfo(link.ToString()) }, cancellationToken);
			_ctx.Set(link);
			return Result.Successful;
		}
	}
}
