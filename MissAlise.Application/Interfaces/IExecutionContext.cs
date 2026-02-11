namespace MissAlise.Application.Interfaces
{
	public interface IExecutionContext
	{
		T Get<T>(string key = null);
		void Set<T>(T value, string key = null);
	}
}
