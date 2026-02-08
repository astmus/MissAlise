
using System;

namespace MissAlise.TelegramBot.Building;

/// <summary>
/// Указывает описание параметра Option для System.CommandLine и справки.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class OptionDescriptionAttribute : Attribute
{
	public string Description { get; }

	public OptionDescriptionAttribute(string description)
	{
		Description = description;
	}
}
