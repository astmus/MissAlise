using System;

namespace MissAlise.TelegramBot.Building.Values;

public abstract record ValueBase
{
	public abstract Type ValueType { get; }
	public object? Default { get; init; }

	/// <summary>Ключ визуального состояния (группа кнопок и т.п.), общий для всех типов значений.</summary>
	public string? VisualStateKey { get; init; }

	public virtual void Accept(IValueVisitor visitor)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		visitor.Visit(this);
	}
}
