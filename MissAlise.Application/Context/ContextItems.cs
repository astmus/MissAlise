using System.Collections.Concurrent;
using MissAlise.Application.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Application.Context
{
	internal class ContextItems : ConcurrentDictionary<string, object>, IContextItems
	{
		public ContextItems() : base(StringComparer.OrdinalIgnoreCase)
		{
			
		}
		public void Set<T>(T value, string key = null) where T : class
			=> AddOrUpdate(key ?? Identity<T>.Discriminator,
					(k, w) => w,
					(k, o, w) => this[k] = w,
					value);

		public T Get<T>(string key = null) where T : class
		{
			if (TryGetValue(key ?? Identity<T>.Discriminator, out var r) && r is T result)
				return result;
			return default;		
		}
	}
}
