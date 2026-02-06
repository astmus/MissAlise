using MissAlise.Wizards.Descriptors;
using MissAlise.Wizards.Demo.BackgroundSync.Steps;

namespace MissAlise.Wizards.Demo.BackgroundSync;

public static class BackgroundSyncWizardDescriptor
{
    public static readonly WizardId Id = WizardId.From("background-sync");

    public static WizardDescriptor Create()
        => new WizardDescriptorBuilder()
            .WithId(Id)
            .StartWith<ChooseModeStep>()
            .AddStep<ChooseFolderStep>()
            .AddStep<ChoosePeriodicityStep>()
            .AddStep<ChooseReportingStep>()
            .AddStep<ConfirmAndToggleStep>()
            .Build();
}
