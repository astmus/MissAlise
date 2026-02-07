namespace MissAlise.Workflow.Demo.BackgroundSync;

public enum BackgroundSyncMode
{
    Full = 0,
    Diff = 1
}

public sealed record BackgroundSyncCommand(
    BackgroundSyncMode Mode,
    bool Force,
    TimeSpan Periodicity,
    string ServerFolder,
    string ClientFolder,
    bool ReportingEnabled,
    bool IsActive);
