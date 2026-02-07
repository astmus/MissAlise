using Microsoft.Extensions.Hosting;
using MissAlise.Workflow.Extensions;

namespace MissAlise.TelegramBot.Workflow;

/// <summary>
/// Ensures WorkflowRegistryOptions.Register is applied after DI container build.
/// </summary>
internal sealed class WorkflowRegistryBootstrapper : IHostedService
{
    private readonly IServiceProvider _sp;

    public WorkflowRegistryBootstrapper(IServiceProvider sp)
    {
        _sp = sp;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sp.UseWorkflowRegistry();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
