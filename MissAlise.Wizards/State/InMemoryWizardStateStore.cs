using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Descriptors;
using MissAlise.Wizards.Runtime;

namespace MissAlise.Wizards.State;

/// <summary>
/// Minimal store for local dev/tests.
/// Key is (chatId,userId).
/// </summary>
public sealed class InMemoryWizardStateStore : IWizardStateStore
{
    private readonly object _lock = new();
    private readonly Dictionary<(long chatId, long userId), WizardSession> _sessions = new();

    private readonly WizardId _defaultWizardId;

    public InMemoryWizardStateStore(WizardId defaultWizardId)
    {
        _defaultWizardId = defaultWizardId;
    }

    public Task<WizardSession> LoadOrCreateAsync(WizardContext context, CancellationToken cancellationToken)
    {
        var key = (context.ChatId, context.UserId);

        lock (_lock)
        {
            if (_sessions.TryGetValue(key, out var session))
                return Task.FromResult(session);

            session = new WizardSession
            {
                ChatId = context.ChatId,
                UserId = context.UserId,
                WizardId = _defaultWizardId,
                CurrentStep = WizardStepId.From("Start") // will be replaced by coordinator caller or registry init
            };

            _sessions[key] = session;
            return Task.FromResult(session);
        }
    }

    public Task SaveAsync(WizardSession session, CancellationToken cancellationToken)
    {
        var key = (session.ChatId, session.UserId);
        lock (_lock)
        {
            _sessions[key] = session;
        }

        return Task.CompletedTask;
    }
}
