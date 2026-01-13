using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Commands;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.Authentication;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot.Handlers
{
	internal class ChatMessageHandler : BotAsyncHandlerBase<Message>
	{
		private readonly IHandleContext _ctx;		
		private readonly ILogger<ChatMessageHandler> _log;
		private readonly IMediator _mm;
		private readonly Bot<UpdateExt> bot;
		private readonly IMapper _mapper;		

		public ChatMessageHandler(IHandleContext ctx, ILogger<ChatMessageHandler> log, IMediator mm, Bot<UpdateExt> bot, IMapper mapper) : base(bot)
		{
			_log = log;
			_mm = mm;
			this.bot = bot;
			_ctx = ctx;
			_mapper = mapper;			
		}

		protected override async Task RunHandleAsync(Message data, CancellationToken cancel)
		{
			Result res = null;
			if (_ctx.GetCurrent<Update>().IsBotCommand())
			{
				var cmd = bot.ParseCommand(data);
				res = await _mm.Send(cmd, cancel).ConfigureAwait(false);
			}
			
		}
	}
} 
