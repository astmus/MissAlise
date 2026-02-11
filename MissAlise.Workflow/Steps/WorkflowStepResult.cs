using MissAlise.Workflow.Descriptors;

namespace MissAlise.Workflow.Steps;

public sealed class WorkflowStepResult
{
	public WorkflowStepId? NextStep { get; init; }
	public IReadOnlyDictionary<string, string>? StateUpdates { get; init; }
	public IReadOnlyList<object>? ProducedCommands { get; init; }
	public bool IsTerminal { get; init; }

	public static WorkflowStepResult Stay(IReadOnlyDictionary<string, string>? updates = null)
		=> new() { StateUpdates = updates };

	public static WorkflowStepResult Next(WorkflowStepId next, IReadOnlyDictionary<string, string>? updates = null)
		=> new() { NextStep = next, StateUpdates = updates };

	public static WorkflowStepResult Next<TStep>(IReadOnlyDictionary<string, string>? updates = null)
		=> Next(WorkflowStepId.From<TStep>(), updates);

	public static WorkflowStepResult Produce(object command, WorkflowStepId? next = null, IReadOnlyDictionary<string, string>? updates = null)
		=> new() { NextStep = next, StateUpdates = updates, ProducedCommands = new[] { command } };

	public static WorkflowStepResult Complete(IReadOnlyDictionary<string, string>? updates = null)
		=> new() { IsTerminal = true, StateUpdates = updates };

	public static WorkflowStepResult Complete(object command, IReadOnlyDictionary<string, string>? updates = null)
		=> new() { IsTerminal = true, StateUpdates = updates, ProducedCommands = new[] { command } };
}
