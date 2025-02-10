using MissAlise.Application.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Application.Context
{
	internal class HandleContext : IHandleContext
	{
		public IContextItems Items { get; init; }

		public HandleContext(IContextItems items)
		{
			Items = items;
		}

		public T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class
		{
			var result =	Items.Get<T>(id ?? Identity<T>.Name);
			if (throwIfNull)
				ArgumentNullException.ThrowIfNull(result);
			return result;
		}
	}
}
