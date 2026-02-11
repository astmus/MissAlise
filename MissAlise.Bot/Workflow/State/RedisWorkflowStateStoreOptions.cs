using System.Text.Json;
using MissAlise.Workflow.Descriptors;

namespace MissAlise.TelegramBot.Workflow.State;

/// <summary>
/// Настройки <see cref="RedisWorkflowStateStore"/>.
/// </summary>
public sealed class RedisWorkflowStateStoreOptions
{
	/// <summary>Префикс ключей Redis (например "workflow" → "workflow:sess:...", "workflow:active:...").</summary>
	public string? KeyPrefix { get; set; }

	/// <summary>Время жизни сессии. При каждом Save сессия продлевается на этот срок (ExpiresAt = UtcNow + DefaultSessionTtl).</summary>
	public required TimeSpan DefaultSessionTtl { get; set; }

	/// <summary>Workflow и шаг по умолчанию при создании новой сессии.</summary>
	public required WorkflowId DefaultWorkflowId { get; set; }

	/// <summary>Шаг по умолчанию при создании новой сессии.</summary>
	public required WorkflowStepId DefaultStartStep { get; set; }
}