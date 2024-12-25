using System.Collections;

namespace MissAlise.ValueObjects.YandexTracker
{
	public interface IReport
	{
		public DateTime From { get; }
		public DateTime To { get; }
		public string Name { get; }
		public string Description { get; }
	}

	public class Report<TRecord> : IEnumerable<TRecord>, IReport where TRecord : class
	{
		public Report(DateTime from, DateTime to, string name, string description)
		{
			From = from;
			To = to;
			Name = name;
			Description = description;
		}
		public IEnumerator<TRecord> GetEnumerator() => Rows.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Rows).GetEnumerator();

		public IEnumerable<TRecord> Rows { get; init; }
		public DateTime From { get; init; }
		public DateTime To { get; init; }
		public string Name { get; init; }
		public string Description { get; }
	}
}
