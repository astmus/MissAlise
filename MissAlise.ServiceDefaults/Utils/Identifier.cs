namespace MissAlise.Utils;

public static class Id<TType>
{
	public static long TypeSeed;
	static long time;
	static int seq = 0;
	public static readonly Guid Identifier;
	public static readonly Type Type;
	public static readonly string Name;
	public static long TimeSeed => Interlocked.Increment(ref time);
	public static int Sequence => Interlocked.Increment(ref seq);
	static Id()
	{
		Type = typeof(TType);
		Identifier = Type.GUID;
		Name = Type.Name;
		TypeSeed = BitConverter.ToInt64(Identifier.ToByteArray());
		time = DateTime.UtcNow.Ticks;
	}
}


