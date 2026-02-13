using System;

namespace MissAlise.TelegramBot.Building.Values;

public abstract record ValueBase
{
	public abstract Type ValueType { get; }
	public object? Default { get; init; }

	public virtual void Accept(IValueVisitor visitor)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		visitor.Visit(this);
	}
}
