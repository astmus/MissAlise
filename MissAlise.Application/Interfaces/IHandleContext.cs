using MissAlise.Application.Common;

namespace MissAlise.Application.Interfaces
{
	public interface IHandleContext : IExecutionContext
	{
		T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class;
	}
}
