namespace MissAlise.ValueObjects.Media;

public readonly record struct FileSize(long Bytes)
{
    public override string ToString() => Bytes.ToString();
}

public sealed record MimeType(string Value)
{
    public override string ToString() => Value;
}

public sealed record ContentHash(string Algorithm, string Value);
