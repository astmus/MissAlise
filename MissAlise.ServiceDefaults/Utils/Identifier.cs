namespace MissAlise.Utils;

public static class Id<TType>
{
	public static long TypeSeed;
	static long timeSeek;
	static int sequenceDiff = 0;
	public static readonly Guid Identifier;
	public static readonly Type Type;
	public static readonly string Name;
	public static readonly string UniqueName;
	public static long TimeSeed => Interlocked.Increment(ref timeSeek);
	public static int Sequence => Interlocked.Increment(ref sequenceDiff);
	static Id()
	{
		Type = typeof(TType);
		ArgumentNullException.ThrowIfNullOrEmpty(UniqueName = Type.FullName ?? string.Empty);
		Identifier = Type.GUID;
		Name = Type.Name;
		TypeSeed = BitConverter.ToInt64(Identifier.ToByteArray());
		timeSeek = DateTime.UtcNow.Ticks;
	}
}