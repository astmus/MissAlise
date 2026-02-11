using System;
using System.Collections.Generic;
using System.Linq;
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
internal sealed class TelegramCliWizardStep : IWorkflowStep
{
    private readonly BotDefinition _bot;

    public TelegramCliWizardStep(BotDefinition bot)
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

        // back to menu
        if (input.Kind == WorkflowInputKind.Callback && string.Equals(input.Payload, "nav:back", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep));

        var path = session.Get(TelegramCliSession.S_Path);
        if (string.IsNullOrWhiteSpace(path) || !_bot.TryGetLeafByPath(path, out var leaf))
        {
            TelegramCliSession.ResetAll(session);
            return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.MenuStep, new Dictionary<string, string>
            {
                [TelegramCliSession.S_Error] = "Сессия ввода параметров устарела. Выбери команду заново."
            }));
        }

        var idx = session.GetInt(TelegramCliSession.S_ParamIndex, 0);
        idx = Math.Clamp(idx, 0, Math.Max(0, leaf.Parameters.Count - 1));

        var inputData = input.Payload;

        // plain text -> set current param
        if (input.Kind == WorkflowInputKind.Text)
            inputData = $"set:{leaf.Parameters[idx].Name}:{input.Text?.Trim()}";

        if (string.IsNullOrWhiteSpace(inputData))
            return Task.FromResult(WorkflowStepResult.Stay());

        if (TelegramCliSession.TryParseSetPayload(inputData, out var pKey, out var pValue))
        {
            if (!TelegramCliSession.TryApplyValue(leaf, session, pKey, pValue, out var error))
            {
                session.Set(TelegramCliSession.S_Error, error ?? "Некорректное значение.");
                session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
                return Task.FromResult(WorkflowStepResult.Stay());
            }

            if (TelegramCliSession.AllRequiredCollected(leaf, session))
            {
                // params are ready -> go to command review
                return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep));
            }

            idx = TelegramCliSession.NextMissingRequiredIndex(leaf, session);
            if (idx < 0) idx = 0;
            session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
            return Task.FromResult(WorkflowStepResult.Stay());
        }

        if (TelegramCliSession.TryParseAdjustPayload(inputData, out var adjKey, out var deltaValue))
        {
            var param = leaf.Parameters.FirstOrDefault(x => x.Name.Equals(adjKey, StringComparison.OrdinalIgnoreCase));
            param ??= leaf.Parameters[idx];
            if (TelegramCliSession.TryAdjustNumeric(session, param, deltaValue, out var adjusted))
                session.Set(TelegramCliSession.ArgKey(param.Name), adjusted);

            session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
            return Task.FromResult(WorkflowStepResult.Stay());
        }

        if (string.Equals(inputData, "nav:accept", StringComparison.OrdinalIgnoreCase))
        {
            var curParam = leaf.Parameters[idx];
            var currentValue = session.Get(TelegramCliSession.ArgKey(curParam.Name));
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                idx = TelegramCliSession.NextMissingRequiredIndex(leaf, session);
                if (idx < 0)
                    return Task.FromResult(WorkflowStepResult.Next(TelegramCliWorkflow.CommandStep));

                session.Set(TelegramCliSession.S_ParamIndex, idx.ToString());
            }

            return Task.FromResult(WorkflowStepResult.Stay());
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
