using MissAlise.Wizards.Descriptors;

namespace MissAlise.Wizards.Registry;

public sealed class WizardRegistry
{
    private readonly Dictionary<WizardId, WizardDescriptor> _items = new();

    public void Register(WizardDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _items[descriptor.Id] = descriptor;
    }

    public WizardDescriptor Get(WizardId id)
        => _items.TryGetValue(id, out var d)
            ? d
            : throw new KeyNotFoundException($"Wizard '{id}' is not registered.");

    public bool TryGet(WizardId id, out WizardDescriptor descriptor)
        => _items.TryGetValue(id, out descriptor!);

    public IReadOnlyCollection<WizardDescriptor> All()
        => _items.Values.ToArray();
}
