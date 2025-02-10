namespace MissAlise.Application.Interfaces
{
	public interface IHandleContext
	{
		IContextItems Items { get; }
		T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class;
	}
}
