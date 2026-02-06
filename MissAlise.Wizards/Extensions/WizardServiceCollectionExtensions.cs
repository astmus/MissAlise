using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.Wizards.Abstractions;
using MissAlise.Wizards.Registry;
using MissAlise.Wizards.Runtime;

namespace MissAlise.Wizards.Extensions;

public static class WizardServiceCollectionExtensions
{
    public static IServiceCollection AddWizards(this IServiceCollection services)
    {
        services.AddSingleton<WizardRegistry>();
        services.AddOptions<WizardRegistryOptions>();

        services.AddScoped<IWizardCoordinator, WizardCoordinator>();

        // Composition root must register a presenter; demo uses BackgroundSyncWizardPresenter
        // but keeping interface here, to avoid hard dependency.
        return services;
    }

    public static IServiceProvider UseWizardRegistry(this IServiceProvider sp)
    {
        var registry = sp.GetRequiredService<WizardRegistry>();
        var opt = sp.GetService<IOptions<WizardRegistryOptions>>()?.Value;

        opt?.Register?.Invoke(registry);

        return sp;
    }
}
