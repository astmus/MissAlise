using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;
using static System.Net.Mime.MediaTypeNames;

namespace MissAlise.TelegramBot.Workflow.Cli;

/// <summary>
/// Формирует UI (текст + инлайн кнопки) для Telegram CLI workflow.
/// Логика переходов живёт в шагах:
/// <see cref="TelegramCliMenuStep"/>, <see cref="TelegramCliWizardStep"/>, <see cref="TelegramCliCommandStep"/>.
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
		var path = session.Get(TelegramCliSession.S_Path);
		var error = session.Get(TelegramCliSession.S_Error);

		if (session.IsCompleted)
			return ResetMenu(session);

		return session.CurrentStep switch
		{
			var s when s == TelegramCliWorkflow.WizardStep
				=> PresentWizard(session, path, error),
			var s when s == TelegramCliWorkflow.CommandStep
				=> PresentCommand(session, path, error),
			_ => PresentMenu(path, error)
		};
	}

	private static WorkflowPresentation ResetMenu(WorkflowSession session)
	{
		session.State.Clear();
		return WorkflowPresentation.Empty;
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
			commands = parent!.SubCommands;
			title = $"/{path}";
			var pathParts = path!.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			parentPath = pathParts.Length > 1 ? string.Join(' ', pathParts.Take(pathParts.Length - 1)) : string.Empty;
		}
		else
		{
			commands = _bot.Definition.Description.SubCommands;
			title = "Выбери команду:";
		}

		lines.Add(title);

		var hasSections = commands.Any(c => c.SubCommands.Any());
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
				buttons.Add(new WorkflowButton { Text = cmd.Command.Title ?? cmd.Name, Payload = "cmd:" + targetPath });
			}
		}
		else
		{
			foreach (var top in commands)
			{
				var targetPath = string.IsNullOrEmpty(path) ? top.Name : path!.TrimEnd() + " " + top.Name;
				buttons.Add(new WorkflowButton { Text = top.Command.Title ?? top.Name, Payload = "cmd:" + targetPath });
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

	private WorkflowPresentation PresentWizard(WorkflowSession session, string? path, string? error)
	{
		if (string.IsNullOrWhiteSpace(path) || !_bot.Definition.TryGetLeafByPath(path, out var leaf))
		{
			return new WorkflowPresentation
			{
				Text = "Сессия ввода параметров устарела. Выбери команду заново.",
				Buttons = _bot.Definition.Description.SubCommands
					.Select(c => new WorkflowButton { Text = "/" + c.Name, Payload = "cmd:" + c.Name })
					.ToArray()
			};
		}

		var idx = session.GetInt(TelegramCliSession.S_ParamIndex, 0);
		idx = Math.Clamp(idx, 0, Math.Max(0, leaf.Parameters.Count - 1));
		var p = leaf.Parameters[idx];
		var currentValue = session.Get(TelegramCliSession.ArgKey(p.Name));
		var currentValueText = string.IsNullOrWhiteSpace(currentValue) ? "не задано" : currentValue;

		var lines = new List<string>();
		if (!string.IsNullOrWhiteSpace(error))
			lines.Add($"Ошибка: {error}");

		lines.Add($"Команда: /{path}");
		lines.Add($"Параметр {idx + 1}/{leaf.Parameters.Count}: {p.Name}");
		if (p.DefaultValue is not null)
			lines.Add($"По умолчанию: {p.DefaultValue}");
		lines.Add($"Значение" + (p.IsRequired ? " (обязательно)" : "") + ":");
		lines.Add($"Текущее значение: {currentValueText}\n");
		
		if (!string.IsNullOrWhiteSpace(p.Description))
			lines.Add(p.Description);

		var buttons = new List<WorkflowButton>();

		// Allowed values -> buttons
		if (p.AllowedValues is { Count: > 0 })
		{
			foreach (var v in p.AllowedValues.Take(10))
			{
				buttons.Add(new WorkflowButton
				{
					Text = v.ToString(),
					Payload = $"set:{p.Name}:{v}"
				});
			}
		}

		buttons.Add(new WorkflowButton { Text = "Отмена", Payload = "nav:cancel" });
		buttons.Add(new WorkflowButton { Text = "← В меню", Payload = "nav:back" });

		return new WorkflowPresentation { Text = string.Join('\n', lines), Buttons = buttons };
	}

	private WorkflowPresentation PresentCommand(WorkflowSession session, string? path, string? hint)
	{
		if (string.IsNullOrWhiteSpace(path) || !_bot.Definition.TryGetLeafByPath(path, out var leaf))
		{
			return new WorkflowPresentation
			{
				Text = "Команда устарела. Выбери её заново.",
				Buttons = _bot.Definition.Description.SubCommands
					.Select(c => new WorkflowButton { Text = "/" + c.Name, Payload = "cmd:" + c.Name })
					.ToArray()
			};
		}

		var lines = new List<string>();
		if (!string.IsNullOrWhiteSpace(hint))
			lines.Add(hint);
		lines.Add($"Команда: /{path}");
		lines.Add("Параметры:");
		foreach (var p in leaf.Parameters)
		{
			var raw = session.Get(TelegramCliSession.ArgKey(p.Name));
			var v = string.IsNullOrWhiteSpace(raw) ? (p.DefaultValue?.ToString() ?? "не задано") : raw;
			var req = p.IsRequired ? " *" : "";
			lines.Add($"- {p.Name}{req}: {v}");
		}

		var buttons = new List<WorkflowButton>();
		// edit buttons per param
		foreach (var p in leaf.Parameters.Take(12))
		{
			buttons.Add(new WorkflowButton
			{
				Text = $"✏ {p.Name}",
				Payload = "nav:edit:" + p.Name
			});
		}
		buttons.Add(new WorkflowButton { Text = "▶ Выполнить", Payload = "nav:run" });
		buttons.Add(new WorkflowButton { Text = "← В меню", Payload = "nav:back" });
		buttons.Add(new WorkflowButton { Text = "Отмена", Payload = "nav:cancel" });

		return new WorkflowPresentation
		{
			Text = string.Join("\n", lines),
			Buttons = buttons
		};
	}
}
