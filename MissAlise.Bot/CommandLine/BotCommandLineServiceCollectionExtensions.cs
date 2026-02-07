using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.TelegramBot.Workflow;
using MissAlise.TelegramBot.Workflow.Cli;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Extensions;
using MissAlise.Workflow.Registry;

namespace MissAlise.TelegramBot.CommandLine;

public static class BotCommandLineServiceCollectionExtensions
{
	/// <summary>Adds CommandLine-based bot commands: first-level = bot commands, second-level = callback, parameter collection via workflow, execution via IWorkflowCommandSink.</summary>
	/// <remarks>Requires workflow core and <see cref="IWorkflowStateStore"/> to be registered (e.g. AddWorkflowCore and a store). Optionally register <see cref="IDefaultWorkflowPresenter"/> for non-CLI workflows.</remarks>
	public static IServiceCollection AddBotCommandLineWorkflow(this IServiceCollection services)
	{
		services.AddSingleton<IBotCommandLineParser, BotCommandLineParser>();
		services.AddScoped<IBotCommandFactory, DefaultBotCommandFactory>();
		services.AddScoped<CollectParameterStep>();
		services.AddScoped<CliParameterCollectionPresenter>();
		services.AddScoped<StubDefaultWorkflowPresenter>();

		services.AddSingleton<IWorkflowResultRenderer, TelegramWorkflowRenderer>();

		services.PostConfigure<WorkflowRegistryOptions>(opt =>
			opt.Register += r => r.Register(CliParameterCollectionWorkflowDescriptor.Create()));

		services.Configure<WorkflowCoordinatorOptions>(opt =>
			opt.DisposeSessionAfterProduce.Add(CliParameterCollectionWorkflowDescriptor.Id));

		services.AddScoped<IWorkflowCommandSink, MediatRWorkflowCommandSink>();

		//services.AddScoped<IWorkflowPresenter>(sp =>
		//	new CompositeWorkflowPresenter(
		//		sp.GetRequiredService<TelegramCliPresenter>(),
		//		sp.GetService<IDefaultWorkflowPresenter>() ?? sp.GetRequiredService<StubDefaultWorkflowPresenter>()));

		return services;
	}
}
