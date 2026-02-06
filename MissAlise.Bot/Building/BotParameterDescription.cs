
using System;
using System.Collections.Generic;

namespace BotDsl;

public sealed record BotParameterDescription(
    string Name,
    string CliName,
    Type ValueType,
    bool IsRequired,
    bool IsOption,
    string? Description,
    IReadOnlyList<string>? AllowedValues,
    string? InlineProviderKey
);
