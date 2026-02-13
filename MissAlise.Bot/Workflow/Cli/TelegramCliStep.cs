using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Steps;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Базовый класс для шагов Telegram CLI (Menu, Wizard, Command).
/// Выносит создание <see cref="TelegramCliSession"/>, снятие ошибки и проверку отмены (cancel).
/// </summary>
internal abstract class TelegramCliStep : IWorkflowStep
{
	protected BotDefinition botDefinition { get; }

	protected TelegramCliStep(BotDefinition bot)
	{
		botDefinition = bot;
	}

	public async Task<WorkflowStepResult> ExecuteAsync(
		WorkflowContext context,
		WorkflowSession session,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		var parsre = botDefinition.RootCommand.Parse(input.InputData);

		var cli = new TelegramCliSession(session);
		cli.RemoveError();

		if (IsCancel(input))
			return OnCancel(cli);

		return await ExecuteCoreAsync(context, cli, input, cancellationToken);
	}

	protected static bool IsCancel(WorkflowInput input)
	{
		if (input.Kind == WorkflowInputKind.Callback && (input.Payload?.Equals("nav:cancel", StringComparison.OrdinalIgnoreCase) == true))
			return true;
		if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
			return string.Equals(input.Text?.Trim(), "/cancel", StringComparison.OrdinalIgnoreCase);
		return false;
	}

	protected string GetCallbackType(string text) => text switch
	{
		var t when t.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase) => "cmd",
		var t when t.StartsWith("set:", StringComparison.OrdinalIgnoreCase) => "set",
		var t when t.StartsWith("adj:", StringComparison.OrdinalIgnoreCase) => "adj",
		var t when t.StartsWith("nav:", StringComparison.OrdinalIgnoreCase) => "nav",

		_ => null
	};

	/// <summary>Вызывается при отмене (cancel). По умолчанию — сброс и переход в меню.</summary>
	protected virtual WorkflowStepResult OnCancel(TelegramCliSession cli)
	{
		cli.ResetAll();
		return WorkflowStepResult.Next<TelegramCliMenuStep>();
	}

	protected abstract Task<WorkflowStepResult> ExecuteCoreAsync(
		WorkflowContext context,
		TelegramCliSession cli,
		WorkflowInput input,
		CancellationToken cancellationToken);
}
