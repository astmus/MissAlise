namespace MissAlise.Enumerators
{
	public ref struct SplitValuesEnumerator
	{
		private ReadOnlySpan<char> _stringTarget;
		private ReadOnlySpan<char> _separators;
		public byte Length
			=> SlicesCount();
		public SplitValuesEnumerator(ReadOnlySpan<char> splitTarget, params char[] separators)
		{
			_stringTarget = splitTarget;
			_separators = separators.AsSpan();
			Current = default;
		}

		public byte SlicesCount()
		{
			byte count = 0;
			var index = 0;
			var span = _stringTarget;

			while ((index = (span = span.Slice(index + 1)).IndexOfAny(_separators)) != -1)
				count++;

			return ++count;
		}

		public bool MoveNext()
		{
			if (_stringTarget.Length == 0) return false;

			var span = _stringTarget;

			var index = span.IndexOfAny(_separators);
			if (index == -1)
			{
				_stringTarget = ReadOnlySpan<char>.Empty;
				Current = new SplitEntry(ReadOnlySpan<char>.Empty, span);
				return true;
			}
			switch (index)
			{
				case > 0:
				if (span[index + 1] is char next && !_separators.Contains(next))
				{
					var delimiter = span.Slice(index, 1);
					span = span[(index + 1)..];
					index = span.IndexOfAny(_separators);
					if (index != -1)
						Current = new SplitEntry(span[..index], delimiter);
					else
						Current = new SplitEntry(span, delimiter);
				}
				return true;
				case 0:
				Current = new SplitEntry(span.Slice(index, 1), span[++index..]);
				_stringTarget = span.Slice(index + Current.Segment.Length);
				return true;
				default:
				return false;
			}
		}
		public SplitValuesEnumerator GetEnumerator() => this;
		public SplitEntry Current { get; private set; }
	}
}
