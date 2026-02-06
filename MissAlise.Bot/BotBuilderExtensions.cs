using BotDsl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;
using MissAlise.TelegramBot.CommandLine;
using MissAlise.TelegramBot.Handlers;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Extensions;
using MissAlise.Workflow.State;
using Telegram.Bot.Types;

namespace MissAlise.TelegramBot;

public static class BotBuilderExtensions
{
	public static IServiceCollection AddBot(this IServiceCollection services, IConfigurationSection botConfig, Action<BotBuilder> configurate)
	{
		var b = new BotBuilder();
		configurate(b);
		services.AddSingleton<BotDefinition>(sp => b.Build());

		services.AddWorkflowCore();
		services.AddSingleton<IWorkflowStateStore>(_ => new InMemoryWorkflowStateStore(
			CliParameterCollectionWorkflowDescriptor.Id,
			WorkflowStepId.From<CollectParameterStep>()));
		services.AddBotCommandLineWorkflow();

		services
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
		services.AddScoped<IAsyncHandler<CallbackQuery>, CallbackQueryHandler>();

		return services;		
	}

	/// <summary>Применяет Workflow registry (дескрипторы workflow). Вызвать после app.Build().</summary>
	public static IServiceProvider UseBotWorkflow(this IServiceProvider serviceProvider)
	{
		return serviceProvider.UseWorkflowRegistry();
	}
}
