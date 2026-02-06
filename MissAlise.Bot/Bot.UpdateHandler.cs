using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Identity;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	internal partial class Bot<TUpdate> where TUpdate: Update
	{
		internal class UpdateHandler : BackgroundService
		{
			private readonly ILogger<UpdateHandler> logger;
			private readonly IServiceScopeFactory factory;
			private readonly Bot<TUpdate> bot;
			private readonly IMapper _mapper;

			public UpdateHandler(
				ILogger<UpdateHandler> logger,
				IServiceScopeFactory factory,
				Bot<TUpdate> bot,
				IMapper mapper)
			{
				this.logger = logger;
				this.factory = factory;
				this.bot = bot;
				_mapper = mapper;
			}

			protected override async Task ExecuteAsync(CancellationToken cancel)
			{
				while (!cancel.IsCancellationRequested)
				{					
					await foreach (var update in bot.Updates.Reader.ReadAllAsync(cancel))
					{
						logger.LogInformation("Got update {Id}", update.Id);
						_ = HandleUpdateAsync(bot.ApiClient, update, cancel);
					}					
				}
			}

			public async Task HandleUpdateAsync(ITelegramBotClient botClient, TUpdate update, CancellationToken cancel)
			{
				try
				{
					using var handleScope = factory.CreateScope();
					var services = handleScope.ServiceProvider;

					var sender = update.GetCurrentUser();
					var ctx = services.GetRequiredService<IHandleContext>();
					ctx.Set(update);
					ctx.Set(_mapper.Map<UserProfile>(sender));
					ctx.Set(sender);
					ctx.Set(update.GetCurrentChat());
					ctx.Set(bot, "bot");

					Task basicTask = (update as UpdateExt) switch
					{
						{ CallbackQuery: not null } => InvokeHandlerAsync(services, update.CallbackQuery, cancel),
						{ InlineQuery: not null } => InvokeHandlerAsync(services, update.InlineQuery, cancel),
						_ => InvokeHandlerAsync(services, update.GetCurrentMessage(), cancel)
					};

					await basicTask.ConfigureAwait(false);
				}
				catch (Exception error)
				{
					logger.LogError(error, error.Message);
				}
			}

			private Task InvokeHandlerAsync<THandleItem>(IServiceProvider sp, THandleItem updateItem, CancellationToken cancel) where THandleItem : class
			{
				var handler = sp.GetRequiredService<IAsyncHandler<THandleItem>>();
				return handler.InvokeAsync(updateItem, cancel);
			}

			private Task HandleSearchAsync(Update update, InlineQuery inlineQuery, object stoppingToken) => throw new NotImplementedException();
			private Task HandleCallbackAsync(Update update, CallbackQuery callbackQuery, object stoppingToken) => throw new NotImplementedException();
		}
	}
}

