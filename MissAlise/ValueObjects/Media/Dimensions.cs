namespace MissAlise.ValueObjects.Media;

public readonly record struct Dimensions(int Width, int Height)
{
    public bool IsEmpty => Width <= 0 || Height <= 0;
    public override string ToString() => $"{Width}x{Height}";
}
