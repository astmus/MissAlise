using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Шаг 2: wizard для добора параметров (кнопки/текст).
/// Переходы:
/// - в <see cref="TelegramCliCommandStep"/> когда собраны все обязательные параметры;
/// - в <see cref="TelegramCliMenuStep"/> при отмене.
/// </summary>
internal sealed class TelegramCliWizardStep : TelegramCliStep
{
	public TelegramCliWizardStep(BotDefinition bot) : base(bot) { }

	protected override Task<WorkflowStepResult> ExecuteCoreAsync(
		WorkflowContext context,
		TelegramCliSession cli,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		if (input.Kind == WorkflowInputKind.Callback && string.Equals(input.Payload, "nav:back", StringComparison.OrdinalIgnoreCase))
			return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep));

		var path = cli.Path;
		if (string.IsNullOrWhiteSpace(path) || !botDefinition.TryGetLeafByPath(path, out var leaf))
		{
			cli.ResetAll();
			return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep, cli.BuildStateUpdate(error: "Сессия ввода параметров устарела. Выбери команду заново.")));
		}

		var idx = Math.Clamp(cli.ParamIndex, 0, Math.Max(0, leaf.Parameters.Count - 1));
		var inputData = input.Payload;

		if (input.Kind == WorkflowInputKind.Text)
			inputData = $"set:{leaf.Parameters[idx].Name}:{input.Text?.Trim()}";

		if (string.IsNullOrWhiteSpace(inputData))
			return Task.FromResult(WorkflowStepResult.Stay());

		if (TelegramCliSession.TryParseChoicePagePayload(inputData, out var choiceKey, out var deltaPage))
		{
			var p = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(choiceKey, StringComparison.OrdinalIgnoreCase))
				?? leaf.Parameters[idx];
			var currentPage = cli.GetChoicePage(p.Name);
			cli.SetChoicePage(p.Name, Math.Max(0, currentPage + deltaPage));
			cli.ParamIndex = idx;
			return Task.FromResult(WorkflowStepResult.Stay());
		}

		if (TelegramCliSession.TryParseAdjustPayload(inputData, out var adjKey, out var deltaValue))
		{
			var param = leaf.Parameters.FirstOrDefault(x => x.Name == adjKey);
			if (param is not null && cli.TryAdjustNumeric(param, deltaValue, out var adjusted))
			{
				cli.SetArg(param.Name, adjusted);
				cli.ParamIndex = idx;
			}
			return Task.FromResult(WorkflowStepResult.Stay());
		}

		if (TelegramCliSession.TryParseSetPayload(inputData, out var pKey, out var pValue))
		{
			if (!cli.TryApplyValue(leaf, pKey, pValue, out var error))
			{
				cli.Error = error ?? "Некорректное значение.";
				cli.ParamIndex = idx;
				return Task.FromResult(WorkflowStepResult.Stay());
			}

			cli.SetChoicePage(pKey, 0);

			if (cli.AllRequiredCollected(leaf))
				return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep));

			idx = cli.NextMissingRequiredIndex(leaf);
			if (idx < 0) idx = 0;
			cli.ParamIndex = idx;
			return Task.FromResult(WorkflowStepResult.Stay());
		}

		if (string.Equals(inputData, "nav:accept", StringComparison.OrdinalIgnoreCase))
		{
			var curParam = leaf.Parameters[idx];
			var currentValue = cli.GetArg(curParam.Name);

			if (string.IsNullOrWhiteSpace(currentValue)
				&& curParam.IsRequired
				&& curParam.Value is not null
				&& curParam.Value.Default is not null)
			{
				cli.SetArg(curParam.Name, Convert.ToString(curParam.Value.Default, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
				currentValue = cli.GetArg(curParam.Name);
			}

			if (!string.IsNullOrWhiteSpace(currentValue) || !curParam.IsRequired)
			{
				idx = cli.NextMissingRequiredIndex(leaf);
				if (idx < 0)
					return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep));

				cli.ParamIndex = idx;
			}

			return Task.FromResult(WorkflowStepResult.Stay());
		}

		return Task.FromResult(WorkflowStepResult.Stay());
	}
}
