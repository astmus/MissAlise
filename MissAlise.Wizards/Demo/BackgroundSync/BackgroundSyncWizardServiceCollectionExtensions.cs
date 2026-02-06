using Microsoft.Extensions.DependencyInjection;
using MissAlise.Wizards.Demo.BackgroundSync.Steps;
using MissAlise.Wizards.Registry;
using MissAlise.Wizards.State;

namespace MissAlise.Wizards.Demo.BackgroundSync;

public static class BackgroundSyncWizardServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundSyncWizard(this IServiceCollection services)
    {
        // Steps
        services.AddScoped<ChooseModeStep>();
        services.AddScoped<ChooseFolderStep>();
        services.AddScoped<ChoosePeriodicityStep>();
        services.AddScoped<ChooseReportingStep>();
        services.AddScoped<ConfirmAndToggleStep>();

        // Presenter (override default presenter in your composition root if needed)
        services.AddScoped<BackgroundSyncWizardPresenter>();

        // Registry registration (composition root should call this once)
        services.PostConfigure<WizardRegistryOptions>(opt =>
        {
            opt.Register += r => r.Register(BackgroundSyncWizardDescriptor.Create());
        });

        // Default in-memory state store for demo (if you don't have your own yet)
        services.AddSingleton<InMemoryWizardStateStore>(_ => new InMemoryWizardStateStore(BackgroundSyncWizardDescriptor.Id));

        return services;
    }
}
