using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MissAlise.Application.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Application.Context
{
	internal class ExecuteContext : ConcurrentDictionary<string, object>, IExecutionContext
	{
		public ExecuteContext() : base(StringComparer.OrdinalIgnoreCase)
		{
			
		}
		public void Set<T>(T value,[CallerMemberName] string key = null)
			=> AddOrUpdate(key ?? Identity<T>.Discriminator,
					(k, w) => w,
					(k, o, w) => this[k] = w,
					value);

		public T Get<T>([CallerMemberName] string key = null)
		{
			if (TryGetValue(key ?? Identity<T>.Discriminator, out var r) && r is T result)
				return result;
			return default;		
		}
	}
}
