using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MissAlise.Application;
using MissAlise.Application.Abstractions;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Models;
using MissAlise.Application.Services.Authentication;
using MissAlise.Application.Services.Sync;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot.Handlers
{
	internal class ChatMessageHandler : BotAsyncHandlerBase<Message>
	{
		private readonly IHandleContext _ctx;
		private readonly IOneDriveService _oneService;
		private readonly ILogger<ChatMessageHandler> log;
		private readonly IMediator mm;
		private readonly IMapper _mapper;
		private readonly IAuthenticationService _authService;

		public ChatMessageHandler(IHandleContext ctx, IOneDriveService oneService, ILogger<ChatMessageHandler> log, IMediator mm, BotWorker bot, IMapper mapper, IAuthenticationService authService) : base(bot)
		{
			this.log = log;
			this.mm = mm;
			_oneService = oneService;
			_ctx = ctx;
			_mapper = mapper;
			_authService = authService;
		}

		protected override async Task HandleAsync(Message data, CancellationToken cancel)
		{
			Result res = null;
			if (data.Text == "/sync")
			{
				res = await mm.Send(new SyncCommand(), cancel).ConfigureAwait(false);
			}

			if (res is not Result<UnauthorizedAccessException> fail)
			{
				var info = await _oneService.GetOwnerInfo(cancel);
				return;
			}

			var tgUser = _ctx.GetCurrent<Telegram.Bot.Types.User>();
			var appUser = _mapper.Map<AppUser>(tgUser);

			if (await _authService.UserExistsAsync(appUser) == false)
			{
				var result = await _authService.AddUserAsync(appUser).ConfigureAwait(false);
				if (!result.Succeeded)
				{
					await _bot.Client.SendMessage(data.Chat, "Ошибка при создании пользователя", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false);
					return;
				}
			}
			
			var link = _oneService.CreateAuthorizeLink(data.Chat.Id);
			await _bot.Client.DeleteMyCommands().ConfigureAwait(false);
			await _bot.Client.SetChatMenuButton(data.Chat.Id, new MenuButtonWebApp() { Text = "Авторизоваться", WebApp = new WebAppInfo(link.ToString()) }, cancel).ConfigureAwait(false);
			await _bot.Client.SendMessage(data.Chat, "Необходима авторизация", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false);
		}
	}
}
