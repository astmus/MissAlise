using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.Building;

public sealed record BotParameterDescription(
    string Name,
    string CliName,
    Type ValueType,
    bool IsRequired,
    bool IsOption,
    string? Description,
    IReadOnlyList<object>? AllowedValues,
    string? InlineProviderKey,
    object? DefaultValue = null
);
