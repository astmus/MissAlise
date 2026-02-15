using System;
using MissAlise.TelegramBot.Building.Values;

namespace MissAlise.TelegramBot.Building;

/// <summary>
/// Описание параметра команды для meta-слоя (CLI + Telegram UI).
/// 
/// Важно: все UI/редакторные настройки параметра (range/step/choices/default и т.п.)
/// инкапсулированы в <see cref="Value"/> как объект типа <see cref="ValueBase"/>,
/// чтобы их можно было обрабатывать через Visitor (Value&lt;int&gt;, Value&lt;bool&gt;, Value&lt;TimeSpan&gt; и т.д.).
/// </summary>
public sealed record BotParameterDescription(
	string Name,
	string CliName,
	Type ValueType,
	bool IsRequired,
	bool IsOption,
	string? Description,
	string? InlineProviderKey,
	ValueBase? Value = null
);
