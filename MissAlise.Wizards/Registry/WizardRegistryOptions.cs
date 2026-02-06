using MissAlise.Wizards.Registry;

namespace MissAlise.Wizards.Registry;

public sealed class WizardRegistryOptions
{
    public Action<WizardRegistry>? Register { get; set; }
}
