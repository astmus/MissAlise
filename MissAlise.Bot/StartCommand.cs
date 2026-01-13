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
using MissAlise.Application.Services.Authentication;
using MissAlise.TelegramBot;
using SQLitePCL;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Application.Commands
{
	public record StartCommand() : ICommand;
	public class StartCommandHandler : AsyncHandlerBase<StartCommand>
	{
		private readonly IHandleContext _ctx;
		private readonly IMapper _mapper;
		private readonly IAuthenticationService _authService;
		private readonly IOneDriveService _oneService;

		public StartCommandHandler(IHandleContext ctx, IMapper mapper, IAuthenticationService authService, IOneDriveService oneService)
		{
			_ctx = ctx;
			_mapper = mapper;
			_authService = authService;
			_oneService = oneService;
		}
		
		protected override async Task HandleAsync(StartCommand data, CancellationToken cancel)
		{
			var tgUser = _ctx.GetCurrent<Telegram.Bot.Types.User>();
			var appUser = _mapper.Map<AppUser>(tgUser);
			var _bot = _ctx.Get<Bot>();
			var _update = _ctx.Get<UpdateExt>("update");
			Result<StartCommand> result = null;

			if (await _authService.UserExistsAsync(appUser) == false)
			{
				var res = await _authService.AddUserAsync(appUser).ConfigureAwait(false);
				if (!res.Succeeded)				
					result = Result<StartCommand>.Fail(string.Join(", ", res.Errors));					
			}
			else
				result = Result<StartCommand>.Ok(data);

			_ctx.Set(result);
			var link = _oneService.CreateAuthorizeLink(_update.Message.Chat.Id);
			_ctx.Set(link);
		}
	}
}
