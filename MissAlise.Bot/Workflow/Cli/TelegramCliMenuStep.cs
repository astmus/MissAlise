using System;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Шаг 1: меню/навигация по дереву команд + парсинг строковых /команд.
/// Переходы:
/// - в <see cref="TelegramCliWizardStep"/>, если не хватает обязательных параметров;
/// - в <see cref="TelegramCliCommandStep"/>, если параметры собраны и можно показать review/execute.
/// </summary>
internal sealed class TelegramCliMenuStep : TelegramCliStep
{
	public TelegramCliMenuStep(BotDefinition bot) : base(bot) { }

	protected override WorkflowStepResult OnCancel(TelegramCliSession cli)
	{
		cli.ResetAll();
		return WorkflowStepResult.Stay();
	}

	protected override Task<WorkflowStepResult> ExecuteCoreAsync(
		WorkflowContext context,
		TelegramCliSession cli,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		// Callback navigation
		if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
		{
			if (TryParseCmdPayload(input.Payload, out var cbPath))
				return Task.FromResult(StartCommandOrWizard(cli, cbPath));
			else
				return Task.FromResult(DisplayMenu(cli));
		}

		// Command line text
		if (input.Kind is WorkflowInputKind.Command or WorkflowInputKind.Text)
		{
			var text = input.Text?.Trim();
			if (string.IsNullOrWhiteSpace(text))
				return Task.FromResult(DisplayMenu(cli));

			if (TryParseCmdPayload(text, out var pastedPath))
				return Task.FromResult(StartCommandOrWizard(cli, pastedPath));

			//if (!text.StartsWith('/'))
			//	return Task.FromResult(DisplayMenu(cli, hint: "Я работаю командами. Выбери команду ниже или введи /start"));

			var parse = botDefinition.RootCommand.Parse(text);
			if (!botDefinition.TryResolveLeaf(parse, out var leaf))
				return Task.FromResult(DisplayMenu(cli, cli.Path));

			foreach (var p in leaf.Parameters)
			{
				var opt = leaf.OptionsByParamKey[p.Name];
				var optRes = parse.CommandResult.FindResultFor(opt);
				if (optRes is null) continue;
				if (optRes.Tokens.Count == 0) continue;
				cli.SetArg(p.Name, string.Join(' ', optRes.Tokens.Select(t => t.Value)));
			}

			cli.Path = leaf.Path;

			var missing = botDefinition.GetMissing(parse, leaf);
			if (missing.Count > 0)
			{
				var firstMissing = missing[0].Param;
				var idx = leaf.Parameters.FindIndex(x => x.Name.Equals(firstMissing.Name, StringComparison.OrdinalIgnoreCase));
				cli.ParamIndex = Math.Max(idx, 0);
				return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
			}
		}

		return Task.FromResult(DisplayMenu(cli));
	}

	private WorkflowStepResult StartCommandOrWizard(TelegramCliSession cli, string path)
	{
		if (botDefinition.TryGetLeafByPath(path, out var leafDesc) && leafDesc.SubCommands.Count == 0)
		{
			cli.Path = BotDefinition.NormalizePath(path);

			if (leafDesc.Parameters.Count == 0)
				return WorkflowStepResult.Next<TelegramCliCommandStep>();

			cli.ParamIndex = 0;
			return WorkflowStepResult.Next<TelegramCliWizardStep>();
		}

		return DisplayMenu(cli, path);
	}

	private static WorkflowStepResult DisplayMenu(TelegramCliSession cli, string? path = null, string? hint = null)
	{
		if (path is not null)
			cli.Path = path;
		if (hint is not null)
			cli.Error = hint;
		cli.RemoveParamIndex();
		return WorkflowStepResult.Stay();
	}
}
