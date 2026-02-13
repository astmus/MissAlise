namespace MissAlise.TelegramBot.Building.Values;

/// <summary>
/// Visitor для объектов описания значения параметра.
/// 
/// Рекомендация: в большинстве случаев достаточно одного метода <see cref="Visit(ValueBase)"/>
/// и pattern matching внутри.
/// 
/// Но если хочется "классический" Visitor — можно реализовать специализированные методы
/// для конкретных типов значений и вызывать их из override <see cref="ValueBase.Accept"/>.
/// </summary>
public interface IValueVisitor
{
	void Visit(ValueBase value);
}
