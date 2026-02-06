using MissAlise.Workflow;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Marker for the default workflow presenter (e.g. BackgroundSync). Register your main presenter as this; CLI composite will use it for non-CLI workflows.</summary>
public interface IDefaultWorkflowPresenter : IWorkflowPresenter
{
}
