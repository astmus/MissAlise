using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>
/// Описание параметра команды, используемое и для CLI-парсинга, и для wizard/inline UI.
/// </summary>
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
