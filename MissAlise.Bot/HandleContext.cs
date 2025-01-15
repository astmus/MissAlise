using MissAlise.Interfaces;
using MissAlise.Utils;

namespace MissAlise.Bot
{
	internal class HandleContext : IHandleContext
	{
		private readonly IServiceProvider services;		
		public IContextItems Items { get; } = new ContextItems();

		public HandleContext(IServiceProvider services)
		{
			this.services = services;			
		}
		
		public T GetCurrent<T>(string id = null) where T : class
			=> Items.Get<T>(id ?? Identity<T>.Name);
	}
}
