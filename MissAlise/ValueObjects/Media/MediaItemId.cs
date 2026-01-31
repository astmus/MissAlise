namespace MissAlise.ValueObjects.Media;

public readonly record struct MediaItemId(string Value)
{    
    public override string ToString() => Value;
	public static implicit operator string(MediaItemId id)
		=> id.Value;
}
