
using System;
using System.Collections.Generic;

namespace BotDsl;

public sealed class BotCommandDescription
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Type? CommandType { get; init; }

    public List<BotCommandDescription> SubCommands { get; } = new();
    public List<BotParameterDescription> Parameters { get; } = new();
}
