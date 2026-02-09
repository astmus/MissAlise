using System.Text.Json;
using System.Text.Json.Serialization;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using StackExchange.Redis;

namespace MissAlise.TelegramBot.Workflow.State;

/// <summary>
/// Хранилище состояния workflow в Redis. Сессия продлевается при каждом сохранении (ExpiresAt).
/// </summary>
public sealed class RedisWorkflowStateStore : IWorkflowStateStore
{
    private readonly IDatabase _db;
    private readonly string _keyPrefix;
    private readonly TimeSpan? _defaultSessionTtl;
    private readonly WorkflowId _defaultWorkflowId;
    private readonly WorkflowStepId _defaultStartStep;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisWorkflowStateStore(
        IConnectionMultiplexer multiplexer,
        RedisWorkflowStateStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _db = multiplexer.GetDatabase(options.Database ?? 0);
        _keyPrefix = string.IsNullOrWhiteSpace(options.KeyPrefix) ? "workflow" : options.KeyPrefix.Trim();
        _defaultSessionTtl = options.DefaultSessionTtl;
        _defaultWorkflowId = options.DefaultWorkflowId;
        _defaultStartStep = options.DefaultStartStep;
        _jsonOptions = options.JsonSerializerOptions ?? CreateDefaultJsonOptions();
    }

    private static JsonSerializerOptions CreateDefaultJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
        };
    }

    private string SessionKey(string sessionId) => $"{_keyPrefix}:sess:{sessionId}";
    private string ActiveKey(long chatId, long userId) => $"{_keyPrefix}:active:{chatId}:{userId}";

    private TimeSpan? GetTtlFromSession(WorkflowSession session)
    {
        if (session.ExpiresAt is not { } expiresAt)
            return null;
        var ttl = expiresAt - DateTimeOffset.UtcNow;
        return ttl > TimeSpan.Zero ? (ttl < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : ttl) : TimeSpan.FromSeconds(1);
    }

    public async Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        var activeKey = ActiveKey(context.ChatId, context.UserId);
        var sidRedis = await _db.StringGetAsync(activeKey).ConfigureAwait(false);

        if (!sidRedis.IsNullOrEmpty && sidRedis.HasValue)
        {
            var sessKey = SessionKey(sidRedis!);
            var json = await _db.StringGetAsync(sessKey).ConfigureAwait(false);
            if (!json.IsNullOrEmpty && json.HasValue && TryDeserialize(json!, out var existing))
                return existing!;
            await _db.KeyDeleteAsync(activeKey).ConfigureAwait(false);
        }

        var session = new WorkflowSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            ChatId = context.ChatId,
            UserId = context.UserId,
            WorkflowId = _defaultWorkflowId,
            CurrentStep = _defaultStartStep,
            ExpiresAt = _defaultSessionTtl.HasValue ? DateTimeOffset.UtcNow + _defaultSessionTtl.Value : null,
        };

        await SaveAsync(session, cancellationToken).ConfigureAwait(false);
        return session;
    }

    public async Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken)
    {
        if (!session.IsCompleted && _defaultSessionTtl.HasValue)
            session.ExpiresAt = DateTimeOffset.UtcNow + _defaultSessionTtl.Value;

        var sessKey = SessionKey(session.SessionId);
        var activeKey = ActiveKey(session.ChatId, session.UserId);
        var payload = JsonSerializer.Serialize(session, _jsonOptions);
        var ttl = GetTtlFromSession(session);

        var setSession = ttl.HasValue
            ? _db.StringSetAsync(sessKey, payload, ttl.Value)
            : _db.StringSetAsync(sessKey, payload);

        if (!session.IsCompleted)
        {
            var setActive = ttl.HasValue
                ? _db.StringSetAsync(activeKey, session.SessionId, ttl.Value)
                : _db.StringSetAsync(activeKey, session.SessionId);
            await setActive.ConfigureAwait(false);
        }
        else
        {
            await _db.KeyDeleteAsync(activeKey).ConfigureAwait(false);
        }

        await setSession.ConfigureAwait(false);
    }

    public async Task DeleteBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;

        var sessKey = SessionKey(sessionId);
        var json = await _db.StringGetAsync(sessKey).ConfigureAwait(false);
        if (!json.IsNullOrEmpty && json.HasValue && TryDeserialize(json!, out var session))
        {
            var activeKey = ActiveKey(session!.ChatId, session.UserId);
            await _db.KeyDeleteAsync(activeKey).ConfigureAwait(false);
        }

        await _db.KeyDeleteAsync(sessKey).ConfigureAwait(false);
    }

    public Task ClearActiveAsync(WorkflowContext context, CancellationToken cancellationToken)
    {
        var activeKey = ActiveKey(context.ChatId, context.UserId);
        return _db.KeyDeleteAsync(activeKey);
    }

    private bool TryDeserialize(string json, out WorkflowSession? session)
    {
        session = null;
        try
        {
            session = JsonSerializer.Deserialize<WorkflowSession>(json, _jsonOptions);
            return session != null;
        }
        catch
        {
            return false;
        }
    }
}
