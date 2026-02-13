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
				await foreach (var update in bot.Updates.Reader.ReadAllAsync(cancel))
				{
					_ = HandleUpdateAsync(update, cancel);
				}
			}

			public async Task HandleUpdateAsync(TUpdate update, CancellationToken cancel)
			{
				logger.LogInformation("Start handle update {Id} {now}", update.Id, DateTime.UtcNow.ToLongTimeString());

				try
				{
					using var handleScope = factory.CreateScope();
					var services = handleScope.ServiceProvider;
					
					var ctx = services.GetRequiredService<IHandleContext>();
					var sender = update.GetCurrentUser();
					var message = update.GetCurrentMessage();
					var chat = update.GetCurrentChat();

					ctx.Set(bot, "bot");
					ctx.Set(update);
					ctx.Set(_mapper.Map<UserProfile>(sender));
					ctx.Set(sender);
					ctx.Set(chat);
					ctx.Set(message);					

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

				logger.LogInformation("End handle update {Id}  {now}", update.Id, DateTime.UtcNow.ToLongTimeString());
			}

			private Task InvokeHandlerAsync<THandleItem>(IServiceProvider sp, THandleItem updateItem, CancellationToken cancel) where THandleItem : class
			{
				var handler = sp.GetRequiredService<IAsyncHandler<THandleItem>>();
				return handler.InvokeAsync(updateItem, cancel);
			}
		}
	}
}

