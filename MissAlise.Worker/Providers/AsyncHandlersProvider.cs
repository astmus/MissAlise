using MissAlise.Interfaces;

namespace MissAlise.Worker.Providers
{
	public interface IAsyncHandlersProvider
	{
		IAsyncHandler<TCommand> GetHandler<TCommand>();
	}

	public class AsyncHandlersProvider : IAsyncHandlersProvider
	{
		private readonly IServiceProvider services;

		public AsyncHandlersProvider(IServiceProvider services)
		{
			this.services = services;
		}

		public IAsyncHandler<TCommand> GetHandler<TCommand>()
		{
			return services.GetService<IAsyncHandler<TCommand>>();
		}
	}
}
