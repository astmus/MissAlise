using MissAlise.TelegramBot.Building.Attributes;
using MissAlise.Workflow.Demo.BackgroundSync;

namespace MissAlise.Workflow.Demo.BotAttrPlayground;

public sealed record ChooseDemoCommand(
	[BotDescription("Формат выгрузки")]
	[BotValueChoose("json", "json", "xml", "csv")]
	string Format,

	[BotDescription("Качество (допустимые значения как кнопки)")]
	[BotValueChoose(80, 10, 30, 50, 80, 100)]
	int Quality
);
public sealed record RangeStepDemoCommand(
	[BotDescription("Громкость (0..100)")]
	[BotRange(0, 100)]
	[BotStep(5)]
	int Volume,

	[BotDescription("Количество попыток (1..10)")]
	[BotRange(1, 10)]
	[BotStep(1)]
	int Retries
);


public sealed record BoolGroupDemoCommand(
	[BotDescription("Режим: только один вариант (группа Mode)")]
	[BotBoolStateGroup("Mode", row: 0)]
	bool ModeOff,

	[BotBoolStateGroup("Mode", row: 0)]
	bool ModeOn,

	[BotBoolStateGroup("Mode", row: 0)]
	bool ModeAuto,

	[BotDescription("Флаги (группа Flags) — можно несколько")]
	[BotBoolStateGroup("Flags", row: 1)]
	bool DryRun,

	[BotBoolStateGroup("Flags", row: 1)]
	bool Verbose,

	[BotBoolStateGroup("Flags", row: 2)]
	bool Force
);

public sealed record SpinTimeDemoCommand(
	[BotDescription("Интервал опроса")]
	[BotValueSpinTime("00:30:00", "00:05:00", "02:00:00", "00:05:00", "{0} min")]
	TimeSpan PollInterval,

	[BotDescription("Таймаут операции")]
	[BotValueSpinTime("00:01:00", "00:00:10", "00:10:00", "00:00:10", "{0} sec")]
	TimeSpan Timeout
);


public sealed record AllInOneDemoCommand(
	[BotDescription("Формат")]
	[BotValueChoose("json", "json", "xml", "csv")]
	string Format,

	[BotDescription("Порог (0..1) шаг 0.05")]
	[BotRange(0.0, 1.0)]
	[BotStep(0.05)]
	double Threshold,

	[BotDescription("Режим: только один")]
	[BotBoolStateGroup("Mode", row: 0)]
	bool Fast,

	[BotBoolStateGroup("Mode", row: 0)]
	bool Balanced,

	[BotBoolStateGroup("Mode", row: 0)]
	bool Safe,

	[BotDescription("Периодичность")]
	[BotValueSpinTime("00:30:00", "00:10:00", "03:20:00", "00:10:00", "{0} min")]
	TimeSpan Periodicity
);

/// <summary>
/// Одна команда для тестирования атрибутов:
/// - BotRangeAttribute
/// - BotStepAttribute
/// - BotBoolStateGroupAttribute
/// </summary>
public sealed record BotAttributesPlaygroundCommand(
	// ===========================
	// INT: Range + Step + Multipliers
	// ===========================

	[BotDescription("Громкость (0..100), шаг 1, ускорение x5/x10/x20")]
	[BotRange(0, 100)]
	[BotStep(1, 5, 10, 20)]
	int Volume,

	[BotDescription("Попытки (1..10), шаг 1")]
	[BotRange(1, 10)]
	[BotStep(1)]
	int Retries,

	// ===========================
	// DOUBLE: Range + Step + Multipliers
	// ===========================

	[BotDescription("Порог (0..1), шаг 0.01, ускорение x10/x25/x50")]
	[BotRange(0.0, 1.0)]
	[BotStep(0.01, 10, 25, 50)]
	double Threshold,

	// ===========================
	// DECIMAL: Range + Step + Multipliers
	// ===========================

	[BotDescription("Цена (0..9999.99), шаг 0.05, ускорение x10/x20")]
	[BotRange(0.00, 9999.99)]
	[BotStep(0.059, 10, 20)]
	decimal Price,

	// ===========================
	// BOOL GROUP #1: YES/NO
	// ===========================

	[BotDescription("Подтверждение: Да/Нет (группа yesno)")]
	[BotBoolStateGroup("yesno")]
	bool Yes,

	[BotBoolStateGroup("yesno")]
	bool No,

	// ===========================
	// BOOL GROUP #2: MODE (Off/On/Auto)
	// ===========================

	[BotDescription("Режим: Off/On/Auto (группа mode)")]
	[BotBoolStateGroup("mode")]
	bool ModeOff,

	[BotBoolStateGroup("mode")]
	bool ModeOn,

	[BotBoolStateGroup("mode")]
	bool ModeAuto
);
