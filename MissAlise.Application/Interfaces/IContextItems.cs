namespace MissAlise.Application.Interfaces
{
	public interface IContextItems
	{
		T Get<T>(string key = null) where T : class;
		void Set<T>(T value, string key = null) where T : class;
	}
}
