using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.Building;
using MissAlise.TelegramBot.Commands;
using MissAlise.TelegramBot.Handlers;
using MissAlise.TelegramBot.Workflow;
using MissAlise.TelegramBot.Workflow.Cli;
using MissAlise.TelegramBot.Workflow.State;
using MissAlise.Workflow;
using MissAlise.Workflow.Extensions;
using MissAlise.Workflow.Registry;
using MissAlise.Workflow.State;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBot(this IServiceCollection services, IConfigurationSection botConfig, Action<BotBuilder> configurate)
		{
			var b = new BotBuilder();
			configurate(b);
			services.AddSingleton<BotDefinition>(sp => b.Build());

			services.AddWorkflowCore();
			//services.AddSingleton<IWorkflowStateStore>(_ => new InMemoryWorkflowStateStore(TelegramCliWorkflow.Id, TelegramCliWorkflow.MenuStep));
			services.AddSingleton<IWorkflowPresenter, TelegramCliPresenter>();
			services.AddScoped<TelegramCliMenuStep>();
			services.AddScoped<TelegramCliWizardStep>();
			services.AddScoped<TelegramCliCommandStep>();
			services.AddScoped<IWorkflowCommandSink, MediatRWorkflowCommandSink>();			
			services.AddScoped<IWorkflowResultRenderer, TelegramWorkflowRenderer>();
			services.AddScoped<IWorkflowResultVisitor, TelegramWorkflowResultVisitor>();
			services.Configure<WorkflowRegistryOptions>(opt => opt.Register = TelegramCliWorkflow.Register);			

			services
				.AddSingleton<Bot>(sp => sp.GetRequiredService<Bot<UpdateExt>>())
				.AddSingleton<Bot<UpdateExt>>()
				.AddHostedService<Bot<UpdateExt>.UpdateReceiver>()
				.AddHostedService<Bot<UpdateExt>.UpdateHandler>()
				.Configure<BotConfiguration>(botConfig);
			services.AddRedisWorkflowStateStore(TelegramCliWorkflow.Id, TelegramCliWorkflow.MenuStep, conf => { conf.DefaultSessionTtl = TimeSpan.FromMinutes(5); });
			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssemblyContaining<StartCommand>();
				cfg.Lifetime = ServiceLifetime.Scoped;
			});

			services.AddScoped<IAsyncHandler<Message>, ChatMessageHandler>();
			services.AddScoped<IAsyncHandler<InlineQuery>, InlineQueryHandler>();
			services.AddScoped<IAsyncHandler<CallbackQuery>, CallbackQueryHandler>();

			return services;
		}

		/// <summary>Применяет Workflow registry (дескрипторы workflow). Вызвать после app.Build().</summary>
		public static IServiceProvider UseBotWorkflow(this IServiceProvider serviceProvider)
		{
			return serviceProvider.UseWorkflowRegistry();
		}

		public static IServiceCollection AddBotService(this IServiceCollection services, IConfigurationSection botConfig, Action<BotBuilder> configurate)
		{			
			// Workflow core + Telegram CLI workflow
			services.AddWorkflowCore();
			services.AddSingleton<IWorkflowStateStore>(_ => new InMemoryWorkflowStateStore(TelegramCliWorkflow.Id, TelegramCliWorkflow.MenuStep));
			services.AddSingleton<IWorkflowPresenter, TelegramCliPresenter>();
			services.AddScoped<TelegramCliMenuStep>();
			services.AddScoped<TelegramCliWizardStep>();
			services.AddScoped<TelegramCliCommandStep>();
			services.AddScoped<IWorkflowCommandSink, MediatRWorkflowCommandSink>();
			services.AddSingleton<TelegramWorkflowRenderer>();
			services.Configure<WorkflowRegistryOptions>(opt => opt.Register = TelegramCliWorkflow.Register);
			services.AddScoped<TelegramCliWorkflow>();

			var b = new BotBuilder();
			configurate(b);
			services.AddSingleton<BotDefinition>(sp => b.Build());

			services
				//.AddSingleton(sp => bot.Build())
				.AddSingleton<Bot>(sp => sp.GetRequiredService<Bot<UpdateExt>>())
				.AddSingleton<Bot<UpdateExt>>()
				.AddHostedService<Bot<UpdateExt>.UpdateReceiver>()
				.AddHostedService<Bot<UpdateExt>.UpdateHandler>()
				.Configure<BotConfiguration>(botConfig);

			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssemblyContaining<StartCommand>();
				cfg.Lifetime = ServiceLifetime.Scoped;
			});

			services.AddScoped<IAsyncHandler<Message>, ChatMessageHandler>();
			services.AddScoped<IAsyncHandler<InlineQuery>, InlineQueryHandler>();
			return services.AddScoped<IAsyncHandler<CallbackQuery>, CallbackQueryHandler>();
		}


		static internal User GetCurrentUser(this Update update)
		{
			var msg = update.GetCurrentMessage();
			return msg?.From ?? update.InlineQuery?.From ?? update.CallbackQuery?.From ?? update.ChosenInlineResult.From;
		}

		static internal bool IsBotCommand(this Update update)
			=> update.Message.Entities?.Any(e => e.Type == Telegram.Bot.Types.Enums.MessageEntityType.BotCommand) == true;

		static internal bool IsBotCommand(this Update update, string commandName)
			=> update.IsBotCommand() && update.Message.Text == commandName;

		static internal Chat GetCurrentChat(this Update update)
			=> update.GetCurrentMessage()?.Chat ?? new Chat() { Id = update.GetCurrentMessage()?.From?.Id ?? update.InlineQuery?.From.Id ?? update.CallbackQuery?.From.Id ?? update.ChosenInlineResult.From.Id };

		static internal Message GetCurrentMessage(this Update update)
			=> update.Message ?? update.CallbackQuery?.Message ?? update.EditedMessage ?? update.ChannelPost ?? update.EditedChannelPost ?? update.Message?.PinnedMessage;

	}

	public static class ReadOnlyListExtensions
	{
		public static int FindIndex<T>(
			this IReadOnlyList<T> list,
			Predicate<T> match)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (match(list[i]))
					return i;
			}

			return -1;
		}
	}
}
