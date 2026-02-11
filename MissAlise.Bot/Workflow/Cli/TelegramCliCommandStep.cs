using System;
using System.Collections.Generic;
using System.Linq;
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
internal sealed class TelegramCliCommandStep : IWorkflowStep
{
    private readonly BotDefinition _bot;

    public TelegramCliCommandStep(BotDefinition bot)
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
            return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep));
        }

        var path = session.Get(TelegramCliSession.S_Path);
        if (string.IsNullOrWhiteSpace(path) || !_bot.TryGetLeafByPath(path, out var leaf))
        {
            TelegramCliSession.ResetAll(session);
            return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep, new Dictionary<string, string>
            {
                [TelegramCliSession.S_Error] = "Команда устарела. Выбери её заново из меню."
            }));
        }

        if (input.Kind == WorkflowInputKind.Callback && input.Payload is not null)
        {
            var p = input.Payload;

            if (string.Equals(p, "nav:back", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep));

            if (string.Equals(p, "nav:run", StringComparison.OrdinalIgnoreCase))
            {
                if (!TelegramCliSession.AllRequiredCollected(leaf, session))
                {
                    // send user to first missing required param
                    var idx = TelegramCliSession.NextMissingRequiredIndex(leaf, session);
                    if (idx < 0) idx = 0;
                    session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
                    session.Set(TelegramCliSession.S_Error, "Не заполнены обязательные параметры.");
                    return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
                }

                var cmd = TelegramCliSession.CreateCommandFromSession(leaf, session);
                TelegramCliSession.ResetAll(session);
                return Task.FromResult(WorkflowStepResult.Complete(cmd));
            }

            // edit specific param: nav:edit:<name>
            if (p.StartsWith("nav:edit:", StringComparison.OrdinalIgnoreCase))
            {
                var name = p.Substring("nav:edit:".Length);
                var idx = leaf.Parameters.FindIndex(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (idx < 0) idx = 0;
                session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
                return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.WizardStep));
            }
        }

        // also allow typing /run
        if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
        {
            var text = input.Text?.Trim();
            if (string.Equals(text, "/run", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(WorkflowStepResult.Stay(new Dictionary<string, string> { [TelegramCliSession.S_Error] = "Нажми ▶ Выполнить." }));
        }

        return Task.FromResult(WorkflowStepResult.Stay());
    }

    private static bool IsCancel(WorkflowInput input)
    {
        if (input.Kind == WorkflowInputKind.Callback && (input.Payload?.Equals("nav:cancel", StringComparison.OrdinalIgnoreCase) == true))
            return true;
        if (input.Kind is WorkflowInputKind.Text or WorkflowInputKind.Command)
            return string.Equals(input.Text?.Trim(), "/cancel", StringComparison.OrdinalIgnoreCase);
        return false;
    }
}
