using MissAlise.Workflow.Descriptors;
using MissAlise.Workflow.Steps;

namespace MissAlise.Workflow;

public sealed class WorkflowSession
{
    public string SessionId { get; init; } = Guid.NewGuid().ToString("N");

    public required long ChatId { get; init; }
    public required long UserId { get; init; }

    public required WorkflowId WorkflowId { get; set; }
    public required WorkflowStepId CurrentStep { get; set; }

    public Dictionary<string, string> State { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsCompleted { get; private set; }

    /// <summary>Момент истечения сессии (UTC). Используется хранилищем для TTL и продления при сохранении.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    public WorkflowSession() { }

    [System.Text.Json.Serialization.JsonConstructor]
    public WorkflowSession(
        string sessionId,
        long chatId,
        long userId,
        WorkflowId workflowId,
        WorkflowStepId currentStep,
        bool isCompleted = false,
        Dictionary<string, string>? state = null,
        DateTimeOffset? expiresAt = null)
    {
        SessionId = string.IsNullOrWhiteSpace(sessionId)
            ? Guid.NewGuid().ToString("N")
            : sessionId;
        ChatId = chatId;
        UserId = userId;
        WorkflowId = workflowId;
        CurrentStep = currentStep;
        IsCompleted = isCompleted;
        ExpiresAt = expiresAt;
        if (state is not null)
        {
            foreach (var (k, v) in state)
                State[k] = v;
        }
    }

    public string? Get(string key) => State.TryGetValue(key, out var v) ? v : null;
    public void Set(string key, string value) => State[key] = value;

    public bool GetBool(string key, bool @default = false)
        => State.TryGetValue(key, out var v) && bool.TryParse(v, out var b) ? b : @default;

    public int GetInt(string key, int @default = 0)
        => State.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : @default;

    public void Apply(WorkflowStepResult result)
    {
        if (result.IsTerminal)
            IsCompleted = true;

        if (result.NextStep is not null)
            CurrentStep = result.NextStep.Value;

        if (result.StateUpdates is not null)
        {
            foreach (var (k, v) in result.StateUpdates)
                State[k] = v;
        }
    }
}
