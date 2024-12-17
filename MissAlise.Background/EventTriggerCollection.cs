using System.Collections.Concurrent;


namespace MissAlise.Background
{
	public class JobDataCollection<TData> : List<TData>
	{
		
	}

	public class EventTriggerCollection : BlockingCollection<EventTrigger>
	{
		public static readonly EventTriggerCollection Instance = new EventTriggerCollection();
	}
}
