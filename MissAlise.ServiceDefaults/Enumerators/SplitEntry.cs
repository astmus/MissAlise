namespace MissAlise.Enumerators
{
	public readonly ref struct SplitEntry
	{
		public SplitEntry(ReadOnlySpan<char> delimiter, ReadOnlySpan<char> segment)
		{
			Segment = segment;
			Delimiter = delimiter;
		}

		public ReadOnlySpan<char> Trimmed(Range sub)
			=> Segment[sub].Trim();

		public ReadOnlySpan<char> Segment { get; }
		public ReadOnlySpan<char> Delimiter { get; }

		public void Deconstruct(out ReadOnlySpan<char> entry, out ReadOnlySpan<char> delimiter)
		{
			entry = Segment;
			delimiter = Delimiter;
		}

		public static implicit operator ReadOnlySpan<char>(SplitEntry entry)
			=> entry.Segment;

		public static implicit operator string(SplitEntry entry)
			=> entry.Segment.ToString();
	}
}
