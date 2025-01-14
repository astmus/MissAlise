using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MissAlise.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Bot
{
	

	internal class HandleContext : IHandleContext
	{
		private readonly IServiceProvider services;
		public string Identifier { get; internal set; }
		public IContextItems Items { get; } = new ContextItems();

		public HandleContext(IServiceProvider services)
		{
			this.services = services;
			Identifier = GetHashCode().ToString();
		}

		public ILogger<TContext> Logger<TContext>()
			=> services.GetService<ILogger<TContext>>();
		public TService GetHandleService<TService>()
			=> services.GetService<TService>();
		public T GetCurrent<T>(string id = null) where T : class
			=> Items.Get<T>(id ?? Identity<T>.Name);
	}
}
