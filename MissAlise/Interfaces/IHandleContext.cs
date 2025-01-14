namespace MissAlise.Interfaces
{
	public interface IHandleContext
	{
		string Identifier { get; }
		IContextItems Items { get; }
		T GetCurrent<T>(string id = null) where T : class;
	}
}
