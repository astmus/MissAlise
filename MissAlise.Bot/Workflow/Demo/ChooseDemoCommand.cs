using System.CommandLine.Parsing;
using System.Globalization;
using System.Reflection;
using MissAlise.TelegramBot.Building.Attributes;

namespace MissAlise.Workflow.Demo.BotAttrPlayground;

public sealed record ChooseDemoCommand(
	[BotParameter("Формат выгрузки", "json", "json", "xml", "csv")]
	string Format,

	[BotParameter("Качество (допустимые значения как кнопки)", 80, 10, 30, 50, 80, 100)]
	int Quality
);

public sealed record RangeStepDemoCommand(
	[BotParameter("Громкость (0..100)", 0, 100, 5)]
	int Volume,

	[BotParameter("Количество попыток (1..10)", 1, 10, 1)]
	int Retries
);

public sealed record BoolGroupDemoCommand(
	[BotParameter("Режим: только один вариант (группа Mode)", "Mode")]
	bool ModeOff,

	[BotParameter("Mode")]
	bool ModeOn,

	[BotParameter("Mode")]
	bool ModeAuto,

	[BotParameter("Флаги (группа Flags) — можно несколько", "Flags")]
	bool DryRun,

	[BotParameter("Flags")]
	bool Verbose,

	[BotParameter("Flags")]
	bool Force
);

public sealed record SpinTimeDemoCommand(
	[BotParameter("Интервал опроса")]
	[BotParameter("00:30:00", "00:05:00", "02:00:00", "00:05:00", "{0} min")]
	TimeSpan PollInterval,

	[BotParameter("Таймаут операции")]
	[BotParameter("00:01:00", "00:00:10", "00:10:00", "00:00:10", "{0} sec")]
	TimeSpan Timeout
);

public sealed record AllInOneDemoCommand(
	[BotParameter("Формат", "json", "json", "xml", "csv")]
	string Format,

	[BotParameter("Порог (0..1) шаг 0.05", 0.0, 1.0, 0.05)]
	double Threshold,

	[BotParameter("Режим: только один", "Mode")]
	bool Fast,

	[BotParameter("Mode")]
	bool Balanced,

	[BotParameter("Mode")]
	bool Safe,

	[BotParameter("Периодичность")]
	[BotParameter("00:30:00", "00:10:00", "03:20:00", "00:10:00", "{0} min")]
	TimeSpan Periodicity,

	[BotParameter("Код повтора (1..5)", 1, 5, 1)]
	int RetryCode
);

/// <summary>
/// Одна команда для тестирования атрибутов (единый BotParameter).
/// </summary>
public sealed record BotAttributesPlaygroundCommand(
	[BotParameter("Громкость (0..100), шаг 1, ускорение x5/x10/x20", 0, 100, 1, 5, 10, 20)]
	int Volume,

	[BotParameter("Попытки (1..10), шаг 1", 1, 10, 1)]
	int Retries,

	[BotParameter("Порог (0..1), шаг 0.01, ускорение x10/x25/x50", 0.0, 1.0, 0.01, 10, 25, 50)]
	double Threshold,

	[BotParameter("Цена (0..9999.99), шаг 0.05, ускорение x10/x20", 0.00, 9999.99, 0.05, 10, 20)]
	decimal Price,

	[BotParameter("Подтверждение: Да/Нет (группа yesno)", "yesno")]
	bool Yes,

	[BotParameter("yesno")]
	bool No,

	[BotParameter("Режим: Off/On/Auto (группа mode)", "mode")]
	bool ModeOff,

	[BotParameter("mode")]
	bool ModeOn,

	[BotParameter("mode")]
	bool ModeAuto
);
