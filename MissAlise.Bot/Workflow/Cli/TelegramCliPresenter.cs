using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using MissAlise.TelegramBot.Building;
using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

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
		var cli = new TelegramCliSession(session);

		if (session.IsCompleted)
			return ResetMenu(session);

		return session.CurrentStep switch
		{
			var s when s == TelegramCliWorkflow.WizardStep
				=> PresentWizard(session, cli),
			var s when s == TelegramCliWorkflow.CommandStep
				=> PresentCommand(session, cli),
			_ => PresentMenu(cli.Path, cli.Error)
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
			Buttons = ChunkButtons(buttons, 3)
		};
	}

	private static IEnumerable<IEnumerable<WorkflowButton>> ChunkButtons(IList<WorkflowButton> buttons, int maxPerRow)
	{
		for (var i = 0; i < buttons.Count; i += maxPerRow)
			yield return buttons.Skip(i).Take(maxPerRow);
	}

	private WorkflowPresentation PresentWizard(WorkflowSession session, TelegramCliSession cli)
	{
		var path = cli.Path;
		if (string.IsNullOrWhiteSpace(path) || !_bot.Definition.TryGetLeafByPath(path, out var leaf))
		{
			return new WorkflowPresentation
			{
				Text = "Сессия ввода параметров устарела. Выбери команду заново.",
				Buttons = new[] { _bot.Definition.Description.SubCommands
					.Select(c => new WorkflowButton { Text = "/" + c.Name, Payload = "cmd:" + c.Name })
					.ToList() }
			};
		}

		var idx = Math.Clamp(cli.ParamIndex, 0, Math.Max(0, leaf.Parameters.Count - 1));
		var p = leaf.Parameters[idx];
		var currentValue = cli.GetArg(p.Name);
		var currentValueText = string.IsNullOrWhiteSpace(currentValue) ? "не задано" : currentValue;

		var lines = new List<string>();
		if (!string.IsNullOrWhiteSpace(cli.Error))
			lines.Add($"Ошибка: {cli.Error}");

		lines.Add($"Команда: /{path}");
		lines.Add($"Параметр {idx + 1}/{leaf.Parameters.Count}: {p.Name}");
		if (p.Value is not null && p.Value.Default is not null)
			lines.Add($"По умолчанию: {p.Value.Default}");
		
		lines.Add($"\n Текущее значение: {currentValueText}\n");
		
		if (!string.IsNullOrWhiteSpace(p.Description))
			lines.Add(p.Description);

		var buttons = new List<WorkflowButton>();

		// Generic editor buttons via Visitor (choices, numeric spinner, bool groups, ...)
		if (p.Value is not null)
		{
			var visitor = new TelegramCliWizardValueButtonsVisitor(cli, p);
			p.Value.Accept(visitor);
			if (visitor.Hints.Count > 0)
				lines.AddRange(visitor.Hints);
			if (visitor.Buttons.Count > 0)
				buttons.AddRange(visitor.Buttons);
		}

		buttons.Add(new WorkflowButton { Text = "Отмена", Payload = "nav:cancel" });
		buttons.Add(new WorkflowButton { Text = "← В меню", Payload = "nav:back" });

		return new WorkflowPresentation { Text = string.Join('\n', lines), Buttons = new[] { buttons } };
	}

	private WorkflowPresentation PresentCommand(WorkflowSession session, TelegramCliSession cli)
	{
		var path = cli.Path;
		if (string.IsNullOrWhiteSpace(path) || !_bot.Definition.TryGetLeafByPath(path, out var leaf))
		{
		return new WorkflowPresentation
		{
			Text = "Команда устарела. Выбери её заново.",
			Buttons = new[] { _bot.Definition.Description.SubCommands
				.Select(c => new WorkflowButton { Text = "/" + c.Name, Payload = "cmd:" + c.Name })
				.ToList() }
		};
	}

	var lines = new List<string>();
		if (!string.IsNullOrWhiteSpace(cli.Error))
			lines.Add(cli.Error);
		lines.Add($"Команда: /{path}");
		lines.Add("Параметры:");
		foreach (var p in leaf.Parameters)
		{
			var raw = cli.GetArg(p.Name);
			var v = string.IsNullOrWhiteSpace(raw)
				? (p.Value?.Default?.ToString() ?? "не задано")
				: raw;
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
			Buttons = new[] { buttons }
		};
	}
}
