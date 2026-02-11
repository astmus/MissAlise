using MissAlise.TelegramBot.Building.Attributes;
using MissAlise.Workflow.Common;

namespace MissAlise.Workflow.Demo.BackgroundSync;

public enum BackgroundSyncMode
{
	Full = 0,
	Diff = 1
}

public sealed record MyCommand(

	//[BotDescription("Запускать полную синхронизацию или частичную")]
	//BackgroundSyncMode Mode,
	//[BotDescription("С удалением фалов на серере")]
	//[BotYesNo]
	//bool Force,
	[BotDescription("Периодичность")]
	[BotValueSpinTime("00:30:00", "00:10:00", "03:20:00", "00:10:00", "10 min")]
	TimeSpan Periodicity,
	string ServerFolder,
	string ClientFolder,
	[BotYesNo]
	bool ReportingEnabled,
	bool IsActive);

public sealed record BackgroundSyncCommand(

	[BotDescription("Запускать полную синхронизацию или частичную")]
	BackgroundSyncMode Mode,
	[BotDescription("С удалением фалов на серере")]
	[BotYesNo]
	bool Force,
	[BotDescription("Периодичность")]
	[BotValueSpinTime("00:30:00", "00:10:00", "03:20:00", "00:10:00", "{0} min")]
	TimeSpan Periodicity,
	string ServerFolder,
	string ClientFolder,
	[BotYesNo]
	bool ReportingEnabled,
	bool IsActive);

internal class BotValueSpinAttribute : Attribute
{
	public BotValueSpinAttribute(string value, string min, string max, string diff, string title)
	{
		Value = value;
		Min = min;
		Max = max;
		Diff = diff;
		Title = title;
	}

	public string Value { get; }
	public string Min { get; }
	public string Max { get; }
	public string Diff { get; }
	public string Title { get; }
}

internal class BotValueSpinTimeAttribute : Attribute
{
	public ValueProvider ValueProvider => new StepValueProvider<TimeSpan>(Value, Min, Max, Diff, Title);
	public BotValueSpinTimeAttribute(string value, string min, string max, string diff, string title)
	{
		Value = TimeSpan.Parse(value);
		Min = TimeSpan.Parse(min);
		Max = TimeSpan.Parse(max);
		Diff = TimeSpan.Parse(diff);		
		Title = title;
		
	}

	public TimeSpan Value { get; }
	public TimeSpan Min { get; }
	public TimeSpan Max { get; }
	public TimeSpan Diff { get; }
	public string Title { get; }
}


