using MissAlise.TelegramBot.Building.Attributes;

namespace MissAlise.Workflow.Demo.BackgroundSync;

public enum BackgroundSyncMode
{
	Full = 0,
	Diff = 1
}

public sealed record MyCommand(
	[BotParameter("Периодичность")]
	[BotParameter("00:30:00", "00:10:00", "03:20:00", "00:10:00", "10 min")]
	TimeSpan Periodicity,
	string ServerFolder,
	string ClientFolder,
	bool ReportingEnabled,
	bool IsActive);

public sealed record BackgroundSyncCommand(
	[BotParameter("Запускать полную синхронизацию или частичную")]
	BackgroundSyncMode Mode,
	[BotParameter("С удалением файлов на сервере")]
	bool Force,
	[BotParameter("Периодичность")]
	[BotParameter("00:30:00", "00:10:00", "03:20:00", "00:10:00", "{0} min")]
	TimeSpan Periodicity,
	string ServerFolder,
	string ClientFolder,
	bool ReportingEnabled,
	bool IsActive);


