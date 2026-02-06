using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Presentation;
using MissAlise.Wizards.Runtime;

namespace MissAlise.Wizards.Demo.BackgroundSync;

public sealed class BackgroundSyncWizardPresenter : IWizardPresenter
{
    public WizardStepPresentation Present(WizardSession session, WizardContext context)
    {
        // Determine which step we're on (by name)
        var step = session.CurrentStep.Value;

        return step switch
        {
            "ChooseModeStep" => Mode(session),
            "ChooseFolderStep" => Folder(session),
            "ChoosePeriodicityStep" => Periodicity(session),
            "ChooseReportingStep" => Reporting(session),
            "ConfirmAndToggleStep" => Confirm(session),
            _ => new WizardStepPresentation
            {
                Text = $"Unknown step '{step}'.",
                Buttons = Array.Empty<WizardButton>()
            }
        };
    }

    private static WizardStepPresentation Mode(WizardSession s)
    {
        var mode = s.Get(BackgroundSyncState.Mode) ?? "not set";
        var force = s.Get(BackgroundSyncState.Force) ?? "not set";

        return new WizardStepPresentation
        {
            Text =
$"Background Sync: выбери режим\n\n" +
$"Mode: {mode}\n" +
$"Force delete downloaded: {force}\n\n" +
"1) Выбери Full/Diff\n2) Выбери Force on/off\n3) После выбора режима мы пойдём дальше.",
            Buttons = new[]
            {
                new WizardButton { Text = "Full", Payload = "bs:mode:full" },
                new WizardButton { Text = "Diff", Payload = "bs:mode:diff" },
                new WizardButton { Text = "Force ON", Payload = "bs:force:on" },
                new WizardButton { Text = "Force OFF", Payload = "bs:force:off" },
            }
        };
    }

    private static WizardStepPresentation Folder(WizardSession s)
    {
        var server = s.Get(BackgroundSyncState.ServerFolder) ?? "<не задано>";
        var client = s.Get(BackgroundSyncState.ClientFolder) ?? "<не задано>";

        return new WizardStepPresentation
        {
            Text =
$"Background Sync: папки\n\n" +
$"Server folder: {server}\n" +
$"Client folder: {client}\n\n" +
"Отправь сообщением: server:/path client:C:\\Path\n" +
"или нажми пресет:",
            Buttons = new[]
            {
                new WizardButton { Text = "Preset: /incoming → C:\\MissAlise\\Incoming", Payload = "bs:folder:preset:1" }
            }
        };
    }

    private static WizardStepPresentation Periodicity(WizardSession s)
    {
        var sec = s.Get(BackgroundSyncState.PeriodSeconds) ?? "<не задано>";

        return new WizardStepPresentation
        {
            Text =
$"Background Sync: периодичность\n\n" +
$"Period (sec): {sec}\n\n" +
"Выбери вариант кнопкой или пришли число секунд сообщением.",
            Buttons = new[]
            {
                new WizardButton { Text = "5 min", Payload = "bs:period:300" },
                new WizardButton { Text = "15 min", Payload = "bs:period:900" },
                new WizardButton { Text = "1 hour", Payload = "bs:period:3600" },
            }
        };
    }

    private static WizardStepPresentation Reporting(WizardSession s)
    {
        var reporting = s.Get(BackgroundSyncState.Reporting) ?? "<не задано>";

        return new WizardStepPresentation
        {
            Text =
$"Background Sync: отчётность\n\n" +
$"Reporting: {reporting}\n\n" +
"Включить аккуратные отчёты в Telegram о загруженных файлах?",
            Buttons = new[]
            {
                new WizardButton { Text = "Reporting ON", Payload = "bs:report:on" },
                new WizardButton { Text = "Reporting OFF", Payload = "bs:report:off" },
            }
        };
    }

    private static WizardStepPresentation Confirm(WizardSession s)
    {
        var mode = s.Get(BackgroundSyncState.Mode) ?? "full";
        var force = s.GetBool(BackgroundSyncState.Force, false);
        var server = s.Get(BackgroundSyncState.ServerFolder) ?? "/";
        var client = s.Get(BackgroundSyncState.ClientFolder) ?? "C:\\";
        var sec = s.GetInt(BackgroundSyncState.PeriodSeconds, 300);
        var reporting = s.GetBool(BackgroundSyncState.Reporting, false);
        var active = s.GetBool(BackgroundSyncState.Active, true);

        var text =
$"Background Sync: подтверждение\n\n" +
$"Mode: {mode}\n" +
$"Force delete downloaded: {force}\n" +
$"Period: {sec} sec\n" +
$"Server folder: {server}\n" +
$"Client folder: {client}\n" +
$"Reporting: {reporting}\n" +
$"Active: {active}\n\n" +
"Нажми Confirm чтобы сформировать команду.\n" +
"Потом кнопкой Toggle можно включать/выключать.";

        return new WizardStepPresentation
        {
            Text = text,
            Buttons = new[]
            {
                new WizardButton { Text = "Confirm", Payload = "bs:confirm" },
                new WizardButton { Text = active ? "Disable" : "Enable", Payload = "bs:toggle" },
            }
        };
    }
}
