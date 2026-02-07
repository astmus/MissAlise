using System;
using System.Collections.Generic;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>
/// Единственный источник правды о дереве команд бота.
/// На базе этого описания строится и System.CommandLine, и Telegram-меню/визарды.
/// </summary>
public sealed class BotCommandDescription
{
    public required string Name { get; init; }
    public string? Description { get; init; }

    /// <summary>
    /// Если задано, узел является leaf-командой: параметры берутся из типа.
    /// Если null — это контейнер (раздел), у которого только подкоманды.
    /// </summary>
    public Type? CommandType { get; init; }

    public List<BotCommandDescription> SubCommands { get; } = new();
    public List<BotParameterDescription> Parameters { get; } = new();
}
