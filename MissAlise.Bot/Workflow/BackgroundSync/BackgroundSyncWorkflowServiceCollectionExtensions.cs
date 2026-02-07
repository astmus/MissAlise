using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.Workflow.Demo.BackgroundSync.Steps;
using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Extensions;
using MissAlise.Workflow.Registry;
using MissAlise.Workflow.State;

namespace MissAlise.Workflow.Demo.BackgroundSync;

public static class BackgroundSyncWorkflowServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundSyncWorkflow(this IServiceCollection services)
    {
        // core services must be registered before
        services.AddScoped<ChooseModeStep>();
        services.AddScoped<ChooseFolderStep>();
        services.AddScoped<ChoosePeriodicityStep>();
        services.AddScoped<ChooseReportingStep>();
        services.AddScoped<ConfirmAndToggleStep>();

        services.AddScoped<BackgroundSyncWorkflowPresenter>();
        services.AddScoped<MissAlise.Workflow.IWorkflowPresenter>(sp => sp.GetRequiredService<BackgroundSyncWorkflowPresenter>());

        services.PostConfigure<WorkflowRegistryOptions>(opt =>
        {
            opt.Register += r => r.Register(BackgroundSyncWorkflowDescriptor.Create());
        });

        // in-memory state (until Mongo implementation)
        services.AddSingleton<MissAlise.Workflow.IWorkflowStateStore>(sp =>
            new InMemoryWorkflowStateStore(
                BackgroundSyncWorkflowDescriptor.Id,
                WorkflowStepId.From<ChooseModeStep>()));

        return services;
    }
}
