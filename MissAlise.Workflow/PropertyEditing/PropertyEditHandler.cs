namespace MissAlise.Workflow.PropertyEditing;

public static class PropertyEditHandler
{
    public static PropertyEditResult TryHandle(WorkflowInput input, WorkflowSession session, PropertyEditDefinition definition, string prefix)
    {
        if (input.Kind != WorkflowInputKind.Callback || string.IsNullOrWhiteSpace(input.Payload))
            return PropertyEditResult.Ignored();

        if (!input.Payload.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase))
            return PropertyEditResult.Ignored();

        var parts = input.Payload.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 3)
            return PropertyEditResult.Ignored();

        var action = parts[1];
        var key = parts[2];
        if (!key.Equals(definition.StateKey, StringComparison.OrdinalIgnoreCase))
            return PropertyEditResult.Ignored();

        return definition.Mode switch
        {
            PropertyEditMode.Options => HandleOptions(parts, session, definition, action),
            PropertyEditMode.Increment => HandleIncrement(session, definition, action),
            _ => PropertyEditResult.Ignored()
        };
    }

    private static PropertyEditResult HandleOptions(string[] parts, WorkflowSession session, PropertyEditDefinition definition, string action)
    {
        if (!action.Equals("set", StringComparison.OrdinalIgnoreCase))
            return PropertyEditResult.Ignored();

        if (parts.Length < 4)
            return PropertyEditResult.Ignored();

        var value = parts[3];
        if (!definition.Options.Contains(value, StringComparer.OrdinalIgnoreCase))
            return PropertyEditResult.Ignored();

        session.Set(definition.StateKey, value);
        return PropertyEditResult.Updated();
    }

    private static PropertyEditResult HandleIncrement(WorkflowSession session, PropertyEditDefinition definition, string action)
    {
        if (action.Equals("accept", StringComparison.OrdinalIgnoreCase))
            return PropertyEditResult.Accepted();

        if (!action.Equals("inc", StringComparison.OrdinalIgnoreCase) && !action.Equals("dec", StringComparison.OrdinalIgnoreCase))
            return PropertyEditResult.Ignored();

        var current = session.GetInt(definition.StateKey, 0);
        var delta = action.Equals("inc", StringComparison.OrdinalIgnoreCase) ? definition.Step : -definition.Step;
        var next = Math.Clamp(current + delta, definition.Min, definition.Max);
        session.Set(definition.StateKey, next.ToString());
        return PropertyEditResult.Updated();
    }
}
