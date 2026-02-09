using System;
using System.Collections.Generic;
using System.Linq;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Формирует UI (текст + инлайн кнопки) для текущего состояния CLI/wizard.
/// Вся логика парсинга/изменения состояния — в <see cref="TelegramCliStep"/>.
/// </summary>
internal sealed class TelegramCliPresenter : IWorkflowPresenter
{
    private readonly Bot _bot;

    public TelegramCliPresenter(Bot bot)
    {
        _bot = bot;
    }

    public WorkflowPresentation Present(WorkflowSession session, WorkflowContext context)
    {
        var mode = session.Get("cli:mode") ?? "menu";
        var path = session.Get("cli:path");
        var error = session.Get("cli:error");

        if (mode == "wizard")
            return PresentWizard(session, path);

        return PresentMenu(path, error);
    }

    private WorkflowPresentation PresentMenu(string? path, string? hint)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(hint))
            lines.Add(hint);

        var buttons = new List<WorkflowButton>();
        IEnumerable<BotCommandDescription> commands;
        string title;
        string? parentPath = null;

        if (!string.IsNullOrWhiteSpace(path) && _bot.Definition.TryGetCommandByPath(path, out var parent))
        {
            commands = parent!.GetAllSubCommands();
            title = $"/{path}";
            var pathParts = path!.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            parentPath = pathParts.Length > 1 ? string.Join(' ', pathParts.Take(pathParts.Length - 1)) : string.Empty;
        }
        else
        {
            commands = _bot.Definition.Description.GetAllSubCommands();
            title = "Выбери команду:";
        }

        lines.Add(title);

        var hasSections = commands.Any(c => !string.IsNullOrEmpty(c.Name));
        if (hasSections)
        {
            string? lastSection = null;
            foreach (var cmd in commands)
            {
                if (!string.IsNullOrEmpty(cmd.Name) && cmd.Name != lastSection)
                {
                    lastSection = cmd.Name;
                    lines.Add($"— {cmd.Name} —");
                }
                if (string.IsNullOrEmpty(cmd.Name))
                    lastSection = null;
                var targetPath = string.IsNullOrEmpty(path) ? cmd.Name : path!.TrimEnd() + " " + cmd.Name;
                buttons.Add(new WorkflowButton { Text = cmd.Name, Payload = "cmd:" + targetPath });
            }
        }
        else
        {
            foreach (var top in commands)
            {
                var targetPath = string.IsNullOrEmpty(path) ? top.Name : path!.TrimEnd() + " " + top.Name;
                buttons.Add(new WorkflowButton { Text = top.Name, Payload = "cmd:" + targetPath });
            }
        }

        if (!string.IsNullOrWhiteSpace(path))
            buttons.Add(new WorkflowButton { Text = "← Назад", Payload = string.IsNullOrEmpty(parentPath) ? "cmd:" : "cmd:" + parentPath });
        return new WorkflowPresentation
        {
            Text = string.Join("\n", lines),
            Buttons = buttons
        };
    }

    private WorkflowPresentation PresentWizard(WorkflowSession session, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !_bot.Definition. TryGetLeafByPath(path, out var leaf))
        {
            return new WorkflowPresentation
            {
                Text = "Сессия ввода параметров устарела. Выбери команду заново.",
                Buttons = _bot.Definition.Description.SubCommands
                    .Select(c => new WorkflowButton { Text = "/" + c.Name, Payload = "cmd:" + c.Name })
                    .ToArray()
            };
        }

        var idx = session.GetInt("cli:paramIndex", 0);
        idx = Math.Clamp(idx, 0, Math.Max(0, leaf.Parameters.Count - 1));
        var p = leaf.Parameters[idx];

        var text = $"Команда: /{leaf.Name}\n" +
                   $"Параметр {idx + 1}/{leaf.Parameters.Count}: {p.Name}\n" +
                   $"Введи значение" + (p.IsRequired ? " (обязательно)" : "") + ":";

        var buttons = new List<WorkflowButton>();

        // Allowed values -> buttons
        if (p.AllowedValues is { Count: > 0 })
        {
            foreach (var v in p.AllowedValues.Take(10))
            {
                buttons.Add(new WorkflowButton
                {
                    Text = v,
                    Payload = $"set:{p.Name}:{v}"
                });
            }
        }

        buttons.Add(new WorkflowButton { Text = "Отмена", Payload = "nav:cancel" });

        return new WorkflowPresentation { Text = text, Buttons = buttons };
    }
}
