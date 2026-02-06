using Microsoft.Extensions.DependencyInjection;
using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Registry;

namespace MissAlise.Wizards.Runtime;

public sealed class WizardCoordinator : IWizardCoordinator
{
    private readonly WizardRegistry _registry;
    private readonly IWizardStateStore _stateStore;
    private readonly IServiceProvider _services;
    private readonly IWizardPresenter _presenter;
    private readonly IWizardCommandSink? _commandSink;

    public WizardCoordinator(
        WizardRegistry registry,
        IWizardStateStore stateStore,
        IServiceProvider services,
        IWizardPresenter presenter,
        IWizardCommandSink? commandSink = null)
    {
        _registry = registry;
        _stateStore = stateStore;
        _services = services;
        _presenter = presenter;
        _commandSink = commandSink;
    }

    public async Task<WizardHandleResult> HandleAsync(
        WizardRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _stateStore.LoadOrCreateAsync(request.Context, cancellationToken);

        var descriptor = _registry.Get(session.WizardId);

        if (!descriptor.Steps.TryGetValue(session.CurrentStep, out var stepType))
            throw new InvalidOperationException($"Wizard '{session.WizardId}' does not contain step '{session.CurrentStep}'.");

        var step = (IWizardStep)_services.GetRequiredService(stepType);

        var stepResult = await step.ExecuteAsync(request.Context, session, request.Input, cancellationToken);

        session.Apply(stepResult);
        await _stateStore.SaveAsync(session, cancellationToken);

        var produced = stepResult.ProducedCommands?.ToArray() ?? Array.Empty<object>();
        if (_commandSink is not null && produced.Length > 0)
        {
            foreach (var cmd in produced)
                await _commandSink.PublishAsync(cmd, request.Context, cancellationToken);
        }

        var presentation = _presenter.Present(session, request.Context);

        return new WizardHandleResult
        {
            Presentation = presentation,
            ProducedCommands = produced
        };
    }
}
