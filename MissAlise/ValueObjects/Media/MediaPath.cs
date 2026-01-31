namespace MissAlise.ValueObjects.Media;

/// <summary>
/// Опционально: путь (или группировка) элемента в хранилище.
/// Хранит "как есть" строку от адаптера (например OneDrive parentReference.path),
/// чтобы можно было фильтровать/группировать.
/// </summary>
public sealed record MediaPath(string Value)
{
    public static readonly MediaPath Empty = new("");

    public override string ToString() => Value;

    public static MediaPath FromNullable(string? value)
        => string.IsNullOrWhiteSpace(value) ? Empty : new MediaPath(value);
}
