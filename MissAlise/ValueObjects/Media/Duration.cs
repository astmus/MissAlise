namespace MissAlise.ValueObjects.Media;

public readonly record struct Duration(TimeSpan Value)
{
    public static Duration FromSeconds(double seconds) => new(TimeSpan.FromSeconds(seconds));
    public override string ToString() => Value.ToString();
}
