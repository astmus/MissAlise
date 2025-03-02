using System.Collections.Concurrent;


namespace MissAlise.Background
{
	public interface IEventTriggersSource : IEnumerable<EventTrigger>
	{
		void Add(EventTrigger item);
	}

	public class EventTriggerCollection : BlockingCollection<EventTrigger>, IEventTriggersSource
	{
	}
}
