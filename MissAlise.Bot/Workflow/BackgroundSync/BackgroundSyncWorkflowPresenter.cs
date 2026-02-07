using MissAlise.Workflow;
using MissAlise.Workflow.Presentation;

namespace MissAlise.Workflow.Demo.BackgroundSync;

public sealed class BackgroundSyncWorkflowPresenter : IWorkflowPresenter
{
    public WorkflowPresentation Present(WorkflowSession session, WorkflowContext context)
    {
        return session.CurrentStep.Value switch
        {
            "ChooseModeStep" => Mode(session),
            "ChooseFolderStep" => Folder(session),
            "ChoosePeriodicityStep" => Periodicity(session),
            "ChooseReportingStep" => Reporting(session),
            "ConfirmAndToggleStep" => Confirm(session),
            _ => new WorkflowPresentation { Text = $"Unknown step '{session.CurrentStep.Value}'." }
        };
    }

    private static WorkflowPresentation Mode(WorkflowSession s)
    {
        var mode = s.Get(BackgroundSyncState.Mode) ?? "not set";
        var force = s.Get(BackgroundSyncState.Force) ?? "not set";

        return new WorkflowPresentation
        {
            Text =
                "Background Sync: выбери режим\n\n" +
                $"Mode: {mode}\n" +
                $"Force delete downloaded: {force}\n\n" +
                "1) Выбери Full/Diff\n2) Выбери Force on/off\n3) После выбора режима мы пойдём дальше.",
            Buttons = new[]
            {
                new WorkflowButton { Text = "Full", Payload = "bs:mode:full" },
                new WorkflowButton { Text = "Diff", Payload = "bs:mode:diff" },
                new WorkflowButton { Text = "Force ON", Payload = "bs:force:on" },
                new WorkflowButton { Text = "Force OFF", Payload = "bs:force:off" },
            }
        };
    }

    private static WorkflowPresentation Folder(WorkflowSession s)
    {
        var server = s.Get(BackgroundSyncState.ServerFolder) ?? "<не задано>";
        var client = s.Get(BackgroundSyncState.ClientFolder) ?? "<не задано>";

        return new WorkflowPresentation
        {
            Text =
                "Background Sync: папки\n\n" +
                $"Server folder: {server}\n" +
                $"Client folder: {client}\n\n" +
                "Отправь сообщением: server:/path client:C:\\Path\n" +
                "или нажми пресет:",
            Buttons = new[]
            {
                new WorkflowButton { Text = "Preset: /incoming → C:\\MissAlise\\Incoming", Payload = "bs:folder:preset:1" }
            }
        };
    }

    private static WorkflowPresentation Periodicity(WorkflowSession s)
    {
        var sec = s.Get(BackgroundSyncState.PeriodSeconds) ?? "<не задано>";
        return new WorkflowPresentation
        {
            Text =
                "Background Sync: периодичность\n\n" +
                $"Period (sec): {sec}\n\n" +
                "Выбери вариант кнопкой или пришли число секунд сообщением.",
            Buttons = new[]
            {
                new WorkflowButton { Text = "5 min", Payload = "bs:period:300" },
                new WorkflowButton { Text = "15 min", Payload = "bs:period:900" },
                new WorkflowButton { Text = "1 hour", Payload = "bs:period:3600" },
            }
        };
    }

    private static WorkflowPresentation Reporting(WorkflowSession s)
    {
        var rep = s.Get(BackgroundSyncState.Reporting) ?? "<не задано>";
        return new WorkflowPresentation
        {
            Text =
                "Background Sync: отчётность\n\n" +
                $"Reporting: {rep}\n\n" +
                "Включить аккуратные отчёты в Telegram о загруженных файлах?",
            Buttons = new[]
            {
                new WorkflowButton { Text = "Reporting ON", Payload = "bs:report:on" },
                new WorkflowButton { Text = "Reporting OFF", Payload = "bs:report:off" },
            }
        };
    }

    private static WorkflowPresentation Confirm(WorkflowSession s)
    {
        var mode = s.Get(BackgroundSyncState.Mode) ?? "full";
        var force = s.GetBool(BackgroundSyncState.Force, false);
        var server = s.Get(BackgroundSyncState.ServerFolder) ?? "/";
        var client = s.Get(BackgroundSyncState.ClientFolder) ?? "C:\\";
        var sec = s.GetInt(BackgroundSyncState.PeriodSeconds, 300);
        var reporting = s.GetBool(BackgroundSyncState.Reporting, false);
        var active = s.GetBool(BackgroundSyncState.Active, true);

        return new WorkflowPresentation
        {
            Text =
                "Background Sync: подтверждение\n\n" +
                $"Mode: {mode}\n" +
                $"Force delete downloaded: {force}\n" +
                $"Period: {sec} sec\n" +
                $"Server folder: {server}\n" +
                $"Client folder: {client}\n" +
                $"Reporting: {reporting}\n" +
                $"Active: {active}\n\n" +
                "Нажми Confirm чтобы сформировать команду.\n" +
                "Потом кнопкой Toggle можно включать/выключать.",
            Buttons = new[]
            {
                new WorkflowButton { Text = "Confirm", Payload = "bs:confirm" },
                new WorkflowButton { Text = active ? "Disable" : "Enable", Payload = "bs:toggle" },
            }
        };
    }
}
