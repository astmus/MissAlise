//namespace MissAlise.TelegramBot.Wizard;
//using System.Text.Json;
//using MissAlise.Workflow;
//using StackExchange.Redis;

//public sealed class RedisWizardSessionStore : IWorkflowStateStore
//{
//	private readonly IDatabase _db;
//	private readonly string _prefix;
//	private readonly TimeSpan _minTtl;
//	private readonly JsonSerializerOptions _json;

//	// Lua: атомарно Упрочитать активную сессию по user+wizardФ, и если бита€ Ч почистить указатель.
//	private static readonly LuaScript GetActiveScript = LuaScript.Prepare(@"
//local activeKey = KEYS[1]
//local sessPrefix = ARGV[1]

//local sid = redis.call('GET', activeKey)
//if (not sid) then
//  return nil
//end

//local sessKey = sessPrefix .. sid
//local json = redis.call('GET', sessKey)
//if (not json) then
//  redis.call('DEL', activeKey)
//  return nil
//end

//return json
//");

//	// Lua: удалить session и, если active указывает на эту session, тоже удалить active
//	private static readonly LuaScript DeleteScript = LuaScript.Prepare(@"
//local sessKey = KEYS[1]
//local activeKey = KEYS[2]
//local sid = ARGV[1]

//local cur = redis.call('GET', activeKey)
//if (cur == sid) then
//  redis.call('DEL', activeKey)
//end

//redis.call('DEL', sessKey)
//return 1
//");

//	public RedisWizardSessionStore(
//		IConnectionMultiplexer mux,
//		string keyPrefix = "missalise",
//		TimeSpan? minTtl = null,
//		JsonSerializerOptions? json = null)
//	{
//		_db = mux.GetDatabase();
//		_prefix = string.IsNullOrWhiteSpace(keyPrefix) ? "missalise" : keyPrefix.Trim();
//		_minTtl = minTtl ?? TimeSpan.FromSeconds(10);

//		_json = json ?? new JsonSerializerOptions
//		{
//			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//			WriteIndented = false
//		};
//	}

//	public async Task<WizardSession?> GetAsync(string sessionId, CancellationToken ct)
//	{
//		if (string.IsNullOrWhiteSpace(sessionId)) return null;

//		var key = SessKey(sessionId);
//		var json = await _db.StringGetAsync(key).ConfigureAwait(false);
//		if (json.IsNullOrEmpty) return null;

//		return Deserialize(json!);
//	}

//	public async Task UpsertAsync(WizardSession session, CancellationToken ct)
//	{
//		if (session is null) throw new ArgumentNullException(nameof(session));
//		if (string.IsNullOrWhiteSpace(session.SessionId)) throw new ArgumentException("SessionId is empty");

//		var ttl = CalcTtl(session.ExpiresAt);
//		var sessKey = SessKey(session.SessionId);
//		var activeKey = ActiveKey(session.UserId, session.WizardKey);

//		var payload = JsonSerializer.Serialize(session, _json);

//		// Pipeline (батч). Ќам не нужна суперстрога€ транзакционность:
//		// session + active пишем УвместеФ, TTL одинаковый. ƒл€ wizard UX это ок.
//		var batch = _db.CreateBatch();

//		var t1 = batch.StringSetAsync(sessKey, payload, expiry: ttl);
//		var t2 = batch.StringSetAsync(activeKey, session.SessionId, expiry: ttl);

//		batch.Execute();
//		await Task.WhenAll(t1, t2).ConfigureAwait(false);
//	}

//	public async Task DeleteAsync(string sessionId, CancellationToken ct)
//	{
//		if (string.IsNullOrWhiteSpace(sessionId)) return;

//		// „тобы удалить active-указатель, нужно знать userId+wizardKey.
//		// ƒостаЄм сессию, если нет Ч просто сносим ключ сессии.
//		var sess = await GetAsync(sessionId, ct).ConfigureAwait(false);
//		if (sess is null)
//		{
//			await _db.KeyDeleteAsync(SessKey(sessionId)).ConfigureAwait(false);
//			return;
//		}

//		var sessKey = SessKey(sessionId);
//		var activeKey = ActiveKey(sess.UserId, sess.WizardKey);

//		// Lua гарантирует: если active указывает на этот sid Ч он будет очищен.
//		await _db.ScriptEvaluateAsync(
//			DeleteScript.ToString(),
//			keys: new RedisKey[] { sessKey, activeKey },
//			values: new RedisValue[] { sessionId }
//		).ConfigureAwait(false);
//	}

//	public async Task<WizardSession?> GetActiveForUserAsync(long userId, string wizardKey, CancellationToken ct)
//	{
//		if (userId <= 0) return null;
//		if (string.IsNullOrWhiteSpace(wizardKey)) return null;

//		var activeKey = ActiveKey(userId, wizardKey);

//		// Lua: GET activeKey -> sid -> GET session -> если нет, activeKey DEL
//		var json = await _db.ScriptEvaluateAsync(
//			GetActiveScript.ToString(),
//			keys: new RedisKey[] { activeKey },
//			values: new RedisValue[] { SessKeyPrefix() }
//		).ConfigureAwait(false);

//		if (json.IsNull) return null;

//		return Deserialize((string)json!);
//	}

//	// --- key helpers ---

//	private string SessKeyPrefix() => $"{_prefix}:wiz:sess:";

//	private string SessKey(string sid) => $"{_prefix}:wiz:sess:{sid}";

//	private string ActiveKey(long userId, string wizardKey)
//		=> $"{_prefix}:wiz:active:{userId}:{wizardKey}";

//	private TimeSpan CalcTtl(DateTimeOffset expiresAt)
//	{
//		var ttl = expiresAt - DateTimeOffset.UtcNow;
//		if (ttl < _minTtl) ttl = _minTtl;
//		return ttl;
//	}

//	private WizardSession? Deserialize(string json)
//	{
//		try
//		{
//			return JsonSerializer.Deserialize<WizardSession>(json, _json);
//		}
//		catch
//		{
//			// ≈сли JSON битый Ч лучше считать, что сессии нет.
//			return null;
//		}
//	}

//	public Task<Runtime.WizardSession> LoadOrCreateAsync(Runtime.WizardContext context, CancellationToken cancellationToken)
//	{
//		throw new NotImplementedException();
//	}

//	public Task SaveAsync(Runtime.WizardSession session, CancellationToken cancellationToken)
//	{
//		throw new NotImplementedException();
//	}

//	public Task<WorkflowSession> LoadOrCreateAsync(WorkflowContext context, CancellationToken cancellationToken)
//	{
//		throw new NotImplementedException();
//	}

//	public Task SaveAsync(WorkflowSession session, CancellationToken cancellationToken)
//	{
//		throw new NotImplementedException();
//	}
//}