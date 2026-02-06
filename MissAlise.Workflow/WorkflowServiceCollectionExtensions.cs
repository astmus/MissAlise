using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.Workflow.Registry;

namespace MissAlise.Workflow.Extensions;

public static class WorkflowServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowCore(this IServiceCollection services)
    {
        services.AddSingleton<WorkflowRegistry>();
        services.AddOptions<WorkflowRegistryOptions>();
        services.AddScoped<IWorkflowCoordinator, WorkflowCoordinator>();
        return services;
    }

    public static IServiceProvider UseWorkflowRegistry(this IServiceProvider sp)
    {
        var registry = sp.GetRequiredService<WorkflowRegistry>();
        var opt = sp.GetService<IOptions<WorkflowRegistryOptions>>()?.Value;
        opt?.Register?.Invoke(registry);
        return sp;
    }
}
