namespace MissAlise.Interfaces
{
	public interface IHandleContext
	{		
		IContextItems Items { get; }
		T GetCurrent<T>(string id = null) where T : class;
	}
}
