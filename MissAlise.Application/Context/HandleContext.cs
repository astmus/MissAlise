using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Application.Context
{
	internal class HandleContext : ContextItems, IHandleContext
	{
		public T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class
		{
			var result = Get<T>(id ?? Identity<T>.Discriminator);

			if (throwIfNull)
				ArgumentNullException.ThrowIfNull(result);

			return result;
		}

		public AppUser CurrentUser
			=> GetCurrent<AppUser>();
	}
}
