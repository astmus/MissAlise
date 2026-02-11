using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
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
internal sealed class TelegramCliMenuStep : IWorkflowStep
{
	private readonly BotDefinition _bot;

	public TelegramCliMenuStep(BotDefinition bot)
	{
		_bot = bot;
	}

	public Task<WorkflowStepResult> ExecuteAsync(
		WorkflowContext context,
		WorkflowSession session,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		session.State.Remove(TelegramCliSession.S_Error);

		if (IsCancel(input))
		{
			TelegramCliSession.ResetAll(session);
			return Task.FromResult(WorkflowStepResult.Stay());
		}

		// Callback navigation
		if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
		{
			if (TelegramCliSession.TryParseCmdPayload(input.Payload, out var cbPath))
				return Task.FromResult(StartCommandOrWizard(session, cbPath));
		}

		// Command line text
		if (input.Kind is WorkflowInputKind.Command or WorkflowInputKind.Text)
		{
			var text = input.Text?.Trim();
			if (string.IsNullOrWhiteSpace(text))
				return Task.FromResult(DisplayMenu(session));

			// allow pasted callback payload
			if (TelegramCliSession.TryParseCmdPayload(text, out var pastedPath))
				return Task.FromResult(StartCommandOrWizard(session, pastedPath));

			if (!text.StartsWith('/'))
				return Task.FromResult(DisplayMenu(session, hint: "Я работаю командами. Выбери команду ниже или введи /start"));

			var parse = _bot.RootCommand.Parse(text);
			if (!_bot.TryResolveLeaf(parse, out var leaf))
			{
				// Not a leaf: show nearest menu
				// (простая эвристика: по текущему пути)
				var path = session.Get(TelegramCliSession.S_Path);
				return Task.FromResult(DisplayMenu(session, path));
			}

			// Prefill provided values into session
			foreach (var p in leaf.Parameters)
			{
				var opt = leaf.OptionsByParamKey[p.Name];
				var optRes = parse.CommandResult.FindResultFor(opt);
				if (optRes is null) continue;
				if (optRes.Tokens.Count == 0) continue;
				session.Set(TelegramCliSession.ArgKey(p.Name), string.Join(' ', optRes.Tokens.Select(t => t.Value)));
			}

			session.Set(TelegramCliSession.S_Path, leaf.Path);

			var missing = _bot.GetMissing(parse, leaf);
			if (missing.Count > 0)
			{
				var firstMissing = missing[0].Param;
				var idx = leaf.Parameters.FindIndex(x => x.Name.Equals(firstMissing.Name, StringComparison.OrdinalIgnoreCase));
				session.Set(TelegramCliSession.S_ParamIndex, Math.Max(idx, 0).ToString());
				return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
			}

			// Everything collected -> go to command review step
			return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep));
		}

		return Task.FromResult(DisplayMenu(session));
	}

	private static bool IsCancel(WorkflowInput input)
	{
		if (input.Kind == WorkflowInputKind.Callback && (input.Payload?.Equals("nav:cancel", StringComparison.OrdinalIgnoreCase) == true))
			return true;
		if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
			return string.Equals(input.Text?.Trim(), "/cancel", StringComparison.OrdinalIgnoreCase);
		return false;
	}

	private WorkflowStepResult StartCommandOrWizard(WorkflowSession session, string path)
	{
		// Leaf -> wizard or command review
		if (_bot.TryGetLeafByPath(path, out var leafDesc) && leafDesc.SubCommands.Count == 0)
		{
			session.Set(TelegramCliSession.S_Path, BotDefinition.NormalizePath(path));

			// no params -> still show command screen with Execute
			if (leafDesc.Parameters.Count == 0)
				return WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep);

			session.Set(TelegramCliSession.S_ParamIndex, "0");
			return WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep);
		}

		return DisplayMenu(session, path);
	}

	private static WorkflowStepResult DisplayMenu(WorkflowSession session, string? path = null, string? hint = null)
	{
		if (path is not null)
			session.Set(TelegramCliSession.S_Path, path);
		if (hint is not null)
			session.Set(TelegramCliSession.S_Error, hint);
		session.State.Remove(TelegramCliSession.S_ParamIndex);
		return WorkflowStepResult.Stay();
	}
}
