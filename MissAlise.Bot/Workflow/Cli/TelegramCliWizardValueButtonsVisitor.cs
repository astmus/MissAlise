using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using MissAlise.TelegramBot.Building;
using MissAlise.TelegramBot.Building.Values;
using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Visitor, который умеет построить набор кнопок для wizard-редактирования параметра
/// на основе meta-описания <see cref="ValueBase"/> (лежит в <see cref="BotParameterDescription.Value"/>).
/// 
/// Поддерживает <see cref="NumericValue{T}"/> (int, long, double, decimal и др. через <see cref="INumber{T}"/>), <see cref="BoolValue"/>, <see cref="StepValue{T}"/>.
/// </summary>
internal sealed class TelegramCliWizardValueButtonsVisitor : IValueVisitor
{
	private readonly TelegramCliSession _cli;
	private readonly BotParameterDescription _param;

	public TelegramCliWizardValueButtonsVisitor(TelegramCliSession cli, BotParameterDescription param)
	{
		_cli = cli ?? throw new ArgumentNullException(nameof(cli));
		_param = param ?? throw new ArgumentNullException(nameof(param));
	}

	public List<WorkflowButton> Buttons { get; } = new();
	public List<string> Hints { get; } = new();

	public void Visit(ValueBase value)
	{
		// 1) Если задан набор допустимых значений — показываем “choice” редактор.
		if (TryBuildChoices(value))
			return;

		switch (value)
		{
			case BoolValue bv:
				BuildBool(bv);
				break;
			default:
				if (TryBuildNumericSpinner(value))
					break;
				if (value is StepValue<TimeSpan> sv)
					BuildTimeSpanSpinner(sv);
				break;
		}
	}

	private bool TryBuildChoices(ValueBase value)
	{
		// Обобщённо читаем Value<T>.AllowedValues через reflection.
		var allowedProp = value.GetType().GetProperty("AllowedValues");
		if (allowedProp is null) return false;
		var allowed = allowedProp.GetValue(value) as System.Collections.IEnumerable;
		if (allowed is null) return false;

		var list = allowed.Cast<object?>().Where(x => x is not null).ToList();
		if (list.Count == 0) return false;

		const int pageSize = 8;
		var page = _cli.GetChoicePage(_param.Name);
		var maxPage = (int)Math.Ceiling(list.Count / (double)pageSize) - 1;
		if (page < 0) page = 0;
		if (page > maxPage) page = maxPage;

		var start = page * pageSize;
		var pageItems = list.Skip(start).Take(pageSize).ToList();
		Hints.Add("Выбери значение:");
		if (list.Count > pageSize)
			Hints.Add($"Страница: {page + 1}/{maxPage + 1}");

		foreach (var item in pageItems)
		{
			var text = item!.ToString() ?? string.Empty;
			var raw = Convert.ToString(item, CultureInfo.InvariantCulture) ?? text;
			Buttons.Add(new WorkflowButton
			{
				Text = text,
				Payload = $"set:{_param.Name}:{raw}"
			});
		}

		if (list.Count > pageSize)
		{
			Buttons.Add(new WorkflowButton
			{
				Text = "◀",
				Payload = $"choicepage:{_param.Name}:-1"
			});
			Buttons.Add(new WorkflowButton
			{
				Text = "▶",
				Payload = $"choicepage:{_param.Name}:1"
			});
		}

		return true;
	}

	private void BuildBool(BoolValue bv)
	{
		var (t, f) = BoolVisualGroups.Get(bv.VisualGroupKey);
		Buttons.Add(new WorkflowButton { Text = t, Payload = $"set:{_param.Name}:true" });
		Buttons.Add(new WorkflowButton { Text = f, Payload = $"set:{_param.Name}:false" });
	}

	private bool TryBuildNumericSpinner(ValueBase value)
	{
		var type = value.GetType();
		if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(NumericValue<>))
			return false;

		var t = type.GetGenericArguments()[0];
		var inumber = typeof(INumber<>).MakeGenericType(t);
		var iparsable = typeof(IParsable<>).MakeGenericType(t);
		if (!inumber.IsAssignableFrom(t) || !iparsable.IsAssignableFrom(t))
			return false;

		var method = typeof(TelegramCliWizardValueButtonsVisitor)
			.GetMethod(nameof(BuildNumericSpinnerCore), BindingFlags.NonPublic | BindingFlags.Instance)!
			.MakeGenericMethod(t);
		method.Invoke(this, new object[] { value });
		return true;
	}

	private void BuildNumericSpinnerCore<T>(NumericValue<T> nv) where T : struct, INumber<T>, IParsable<T>
	{
		var fp = CultureInfo.InvariantCulture;
		var currentRaw = _cli.GetArg(_param.Name);
		var current = string.IsNullOrWhiteSpace(currentRaw)
			? nv.GetDefaultOr(T.AdditiveIdentity)
			: T.Parse(currentRaw!, fp);
		current = nv.Clamp(current);

		var multipliers = new[] { 1 }
			.Concat(nv.Multipliers ?? Array.Empty<int>())
			.Where(x => x >= 1)
			.Distinct()
			.Take(6)
			.ToArray();

		if (nv.Min is { } || nv.Max is { })
		{
			var minText = nv.Min is { } mi ? mi.ToString(null, fp) : "-∞";
			var maxText = nv.Max is { } ma ? ma.ToString(null, fp) : "+∞";
			Hints.Add($"Диапазон: {minText} .. {maxText}");
		}

		foreach (var m in multipliers)
		{
			var step = nv.StepBase * T.CreateChecked(m);
			var prev = nv.Clamp(current - step);
			var next = nv.Clamp(current + step);

			Buttons.Add(new WorkflowButton
			{
				Text = $"◀ {prev.ToString(null, fp)}",
				Payload = $"adj:{_param.Name}:{(-step).ToString(null, fp)}"
			});
			Buttons.Add(new WorkflowButton
			{
				Text = $"{next.ToString(null, fp)} ▶",
				Payload = $"adj:{_param.Name}:{step.ToString(null, fp)}"
			});
		}

		Buttons.Add(new WorkflowButton { Text = "✅ Принять", Payload = "nav:accept" });
	}

	private void BuildTimeSpanSpinner(StepValue<TimeSpan> sv)
	{
		var currentRaw = _cli.GetArg(_param.Name);
		var current = string.IsNullOrWhiteSpace(currentRaw)
			? ((TimeSpan?)sv.Default ?? TimeSpan.Zero)
			: TimeSpan.Parse(currentRaw!, CultureInfo.InvariantCulture);

		var baseStep = sv.StepBase;
		var multipliers = new[] { 1 }
			.Concat(sv.Multipliers ?? Array.Empty<int>())
			.Where(x => x >= 1)
			.Distinct()
			.Take(6)
			.ToArray();

		if (sv.Min is { } min || sv.Max is { } max)
		{
			var minText = sv.Min is { } mi ? mi.ToString("c", CultureInfo.InvariantCulture) : "-∞";
			var maxText = sv.Max is { } ma ? ma.ToString("c", CultureInfo.InvariantCulture) : "+∞";
			Hints.Add($"Диапазон: {minText} .. {maxText}");
		}

		foreach (var m in multipliers)
		{
			var step = TimeSpan.FromTicks(baseStep.Ticks * m);
			var prev = current - step;
			var next = current + step;
			Buttons.Add(new WorkflowButton { Text = $"◀ {prev.ToString("c", CultureInfo.InvariantCulture)}", Payload = $"adj:{_param.Name}:{(-step).ToString("c", CultureInfo.InvariantCulture)}" });
			Buttons.Add(new WorkflowButton { Text = $"{next.ToString("c", CultureInfo.InvariantCulture)} ▶", Payload = $"adj:{_param.Name}:{step.ToString("c", CultureInfo.InvariantCulture)}" });
		}
		Buttons.Add(new WorkflowButton { Text = "✅ Принять", Payload = "nav:accept" });
	}
}
