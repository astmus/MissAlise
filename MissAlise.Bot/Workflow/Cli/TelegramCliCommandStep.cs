using System;
using System.Threading;
using System.Threading.Tasks;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Шаг 3: экран команды (review/execute).
/// - показывает текущие параметры
/// - позволяет перейти в wizard для редактирования конкретного параметра
/// - выполняет команду при nav:run
/// </summary>
internal sealed class TelegramCliCommandStep : TelegramCliStep
{
	public TelegramCliCommandStep(BotDefinition bot) : base(bot) { }

	protected override Task<WorkflowStepResult> ExecuteCoreAsync(
		WorkflowContext context,
		TelegramCliSession cli,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		var path = cli.Path;
		if (string.IsNullOrWhiteSpace(path) || !botDefinition.TryGetLeafByPath(path, out var leaf))
		{
			cli.ResetAll();
			return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep, cli.BuildStateUpdate(error: "Команда устарела. Выбери её заново из меню.")));
		}

		if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
		{
			var p = input.Payload;

			if (string.Equals(p, "nav:back", StringComparison.OrdinalIgnoreCase))
				return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep));

			if (string.Equals(p, "nav:run", StringComparison.OrdinalIgnoreCase))
			{
				if (!cli.AllRequiredCollected(leaf))
				{
					var idx = cli.NextMissingRequiredIndex(leaf);
					if (idx < 0) idx = 0;
					cli.ParamIndex = idx;
					cli.Error = "Не заполнены обязательные параметры.";
					return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
				}

				var cmd = cli.CreateCommandFromSession(leaf);
				cli.ResetAll();
				return Task.FromResult(WorkflowStepResult.Complete(cmd));
			}

			if (p.StartsWith("nav:edit:", StringComparison.OrdinalIgnoreCase))
			{
				var name = p.Substring("nav:edit:".Length);
				var idx = leaf.Parameters.FindIndex(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
				if (idx < 0) idx = 0;
				cli.ParamIndex = idx;
				return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
			}
		}

		if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
		{
			var text = input.Text?.Trim();
			if (string.Equals(text, "/run", StringComparison.OrdinalIgnoreCase))
				return Task.FromResult(WorkflowStepResult.Stay(cli.BuildStateUpdate(error: "Нажми ▶ Выполнить.")));
		}

		return Task.FromResult(WorkflowStepResult.Stay());
	}
}
