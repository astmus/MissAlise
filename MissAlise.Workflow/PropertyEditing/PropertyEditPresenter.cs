using MissAlise.Workflow.Presentation;

namespace MissAlise.Workflow.PropertyEditing;

public sealed class PropertyEditPresenter
{
    public WorkflowPresentation Present(WorkflowSession session, PropertyEditDefinition definition, string prefix)
    {
        var current = session.Get(definition.StateKey);
        var formatted = definition.FormatValue?.Invoke(current) ?? (string.IsNullOrWhiteSpace(current) ? "<не задано>" : current);

        var text =
            $"{definition.Title}\n\n" +
            $"{definition.ValueLabel}: {formatted}";

        var buttons = new List<WorkflowButton>();
        switch (definition.Mode)
        {
            case PropertyEditMode.Options:
                foreach (var option in definition.Options)
                {
                    buttons.Add(new WorkflowButton
                    {
                        Text = option,
                        Payload = $"{prefix}:set:{definition.StateKey}:{option}"
                    });
                }
                break;
            case PropertyEditMode.Increment:
                buttons.Add(new WorkflowButton
                {
                    Text = definition.DecrementText,
                    Payload = $"{prefix}:dec:{definition.StateKey}"
                });
                buttons.Add(new WorkflowButton
                {
                    Text = definition.IncrementText,
                    Payload = $"{prefix}:inc:{definition.StateKey}"
                });
                break;
        }

        if (definition.ShowAccept)
        {
            buttons.Add(new WorkflowButton
            {
                Text = definition.AcceptText,
                Payload = $"{prefix}:accept:{definition.StateKey}"
            });
        }

        return new WorkflowPresentation
        {
            Text = text,
            Buttons = buttons
        };
    }
}
