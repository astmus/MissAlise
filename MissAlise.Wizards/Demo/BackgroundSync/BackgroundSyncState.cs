namespace MissAlise.Wizards.Demo.BackgroundSync;

public static class BackgroundSyncState
{
    public const string Mode = "mode";                 // full|diff
    public const string Force = "force";               // true|false
    public const string ServerFolder = "serverFolder"; // string
    public const string ClientFolder = "clientFolder"; // string
    public const string PeriodSeconds = "periodSec";   // int
    public const string Reporting = "reporting";       // true|false
    public const string Active = "active";             // true|false
}
