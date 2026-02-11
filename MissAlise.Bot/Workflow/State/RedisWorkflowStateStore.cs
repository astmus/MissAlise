using System.Text.Json;
using System.Text.Json.Serialization;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using StackExchange.Redis;

namespace MissAlise.TelegramBot.Workflow.State;

public sealed class RedisWorkflowStateStore : IWorkflowStateStore
{
	private readonly IDatabase _db;
	private readonly string _keyPrefix;
	private readonly TimeSpan _defaultSessionTtl;
	private readonly WorkflowId _defaultWorkflowId;
	private readonly WorkflowStepId _defaultStartStep;
	private readonly JsonSerializerOptions _jsonOptions;

	public RedisWorkflowStateStore(
		IConnectionMultiplexer multiplexer,
		RedisWorkflowStateStoreOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		_db = multiplexer.GetDatabase();
		_keyPrefix = string.IsNullOrWhiteSpace(options.KeyPrefix) ? "workflow" : options.KeyPrefix.Trim();
		_defaultSessionTtl = options.DefaultSessionTtl;
		_defaultWorkflowId = options.DefaultWorkflowId;
		_defaultStartStep = options.DefaultStartStep;
		_jsonOptions = DefaultJsonOptions();
	}

	static JsonSerializerOptions _options;
	private static JsonSerializerOptions DefaultJsonOptions()
	{
		return _options ?? (_options ?? new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNameCaseInsensitive = true,
		});
	}

	private string SessionKey(string sessionId) => $"{_keyPrefix}:sess:{sessionId}";
	private string ActiveKey(long chatId, long userId) => $"{_keyPrefix}:active:{chatId}:{userId}";

	public async Task<bool> WasExpiredAsync(WorkflowContext context, CancellationToken ct)
	{
		var activeKey = ActiveKey(context.ChatId, context.UserId);
		var sid = await _db.StringGetAsync(activeKey).ConfigureAwait(false);

		if (sid.IsNullOrEmpty)
			return false;

		var sessKey = SessionKey(sid!);
		var exists = await _db.KeyExistsAsync(sessKey).ConfigureAwait(false);

		return !exists;
	}

	private TimeSpan GetTtlFromSession(WorkflowSession session)
	{
		ArgumentNullException.ThrowIfNull(session.ExpiresAt, nameof(session.ExpiresAt));
		return session.ExpiresAt - DateTimeOffset.UtcNow;
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
			ExpiresAt = DateTimeOffset.UtcNow.Add(_defaultSessionTtl)
		};

		await SaveAsync(session, cancellationToken).ConfigureAwait(false);
		return session;
	}

	public async Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken)
	{
		if (!session.IsCompleted)
			session.ExpiresAt = DateTimeOffset.UtcNow.Add(_defaultSessionTtl);

		var sessKey = SessionKey(session.SessionId);
		var activeKey = ActiveKey(session.ChatId, session.UserId);

		if (session.IsCompleted)
		{
			var tDel1 = _db.KeyDeleteAsync(activeKey);
			var tDel2 = _db.KeyDeleteAsync(sessKey);
			await Task.WhenAll(tDel1, tDel2).ConfigureAwait(false);
			return;
		}

		var payload = JsonSerializer.Serialize(session, _jsonOptions);
		var ttl = GetTtlFromSession(session);

		var batch = _db.CreateBatch();

		var tSetSession = batch.StringSetAsync(sessKey, payload, expiry: ttl);
		var tSetActive = batch.StringSetAsync(activeKey, session.SessionId, expiry: ttl);			

		batch.Execute();

		await Task.WhenAll(tSetSession, tSetActive).ConfigureAwait(false);
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
