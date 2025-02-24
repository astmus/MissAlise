using MissAlise.Application.Models;

namespace MissAlise.Application.Interfaces
{
	public interface IHandleContext : IContextItems
	{		
		AppUser CurrentUser { get; }
		T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class;
	}
}
