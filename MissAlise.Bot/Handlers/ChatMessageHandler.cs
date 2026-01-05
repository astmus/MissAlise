using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
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

		public ChatMessageHandler(IHandleContext ctx, IOneDriveService oneService, ILogger<ChatMessageHandler> log, IMediator mm, Bot<UpdateExt> bot, IMapper mapper, IAuthenticationService authService) : base(bot)
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
			if (_ctx.GetCurrent<Update>() is UpdateExt update && update.Command is ICommand cmd)			
				res = await mm.Send(cmd, cancel).ConfigureAwait(false);

			var info = await _oneService.GetOwnerInfo(cancel);
			if (res is not Result<UnauthorizedAccessException> fail)
			{
				return;
			}

			// нижележащее по хорошему бы в базовый класс так как "неавторизованность" может и не с Message начинаться
			var tgUser = _ctx.GetCurrent<Telegram.Bot.Types.User>();
			var appUser = _mapper.Map<AppUser>(tgUser);
			if (await _authService.UserExistsAsync(appUser) == false)
			{
				var result = await _authService.AddUserAsync(appUser).ConfigureAwait(false);
				if (!result.Succeeded)
				{
					await _bot.ApiClient.SendMessage(data.Chat, "Ошибка при создании пользователя", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false);
					return;
				}
			}

			var link = _oneService.CreateAuthorizeLink(data.Chat.Id);
			await _bot.ApiClient.DeleteMyCommands().ConfigureAwait(false);
			await _bot.ApiClient.SetChatMenuButton(data.Chat.Id, new MenuButtonWebApp() { Text = "Авторизоваться", WebApp = new WebAppInfo(link.ToString()) }, cancel).ConfigureAwait(false);
			await _bot.ApiClient.SendMessage(data.Chat, "Необходима авторизация", parseMode: ParseMode.MarkdownV2, cancellationToken: cancel).ConfigureAwait(false);
		}
	}
}
