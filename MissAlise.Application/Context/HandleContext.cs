using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Application.Context
{
	internal class HandleContext : ConcurrentDictionary<string, object>, IHandleContext
	{
		public HandleContext() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public void Set<T>(T value, string key = null)
			=> AddOrUpdate(key ?? Identity<T>.Discriminator,
					(k, w) => w,
					(k, o, w) => this[k] = w,
					value);

		public T Get<T>(string key = null)
		{
			if (TryGetValue(key ?? Identity<T>.Discriminator, out var r) && r is T result)
				return result;
			return default;
		}
		public T GetCurrent<T>(string id = null, bool throwIfNull = false) where T : class
		{
			var result = Get<T>(id ?? Identity<T>.Discriminator);

			if (throwIfNull)
				ArgumentNullException.ThrowIfNull(result);

			return result;
		}
	}
}
