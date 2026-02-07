using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow;

public sealed class WorkflowSession
{
	public required long ChatId { get; init; }
	public required long UserId { get; init; }

	public required WorkflowId WorkflowId { get; set; }
	public required WorkflowStepId CurrentStep { get; set; }

	public Dictionary<string, string> State { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// For object initializer (e.g. new WorkflowSession { ChatId = ..., ... }).
	/// </summary>
	public WorkflowSession() { }

	/// <summary>
	/// For JSON deserialization (e.g. Redis). State is populated from the deserialized dictionary.
	/// </summary>
	[System.Text.Json.Serialization.JsonConstructor]
	public WorkflowSession(long chatId, long userId, WorkflowId workflowId, WorkflowStepId currentStep, Dictionary<string, string>? state = null)
	{
		ChatId = chatId;
		UserId = userId;
		WorkflowId = workflowId;
		CurrentStep = currentStep;
		if (state == null) return;

		foreach (var (k, v) in state)
			State[k] = v;

	}

	public string? Get(string key) => State.TryGetValue(key, out var v) ? v : null;
	public void Set(string key, string value) => State[key] = value;

	public bool GetBool(string key, bool @default = false)
		=> State.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : @default;

	public int GetInt(string key, int @default = 0)
		=> State.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : @default;

	public void Apply(WorkflowStepResult result)
	{
		if (result.NextStep is not null)
			CurrentStep = result.NextStep.Value;

		if (result.StateUpdates is not null)
		{
			foreach (var (k, v) in result.StateUpdates)
				State[k] = v;
		}
	}
}
