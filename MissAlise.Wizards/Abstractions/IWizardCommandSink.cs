namespace MissAlise.Wizards.Abstractions;

/// <summary>
/// Abstraction over "sending a command somewhere" (message bus, mediator, etc).
/// In Telegram integration you can also use it to route created commands to Background service.
/// </summary>
public interface IWizardCommandSink
{
    Task PublishAsync(object command, WizardContext context, CancellationToken cancellationToken);
}
