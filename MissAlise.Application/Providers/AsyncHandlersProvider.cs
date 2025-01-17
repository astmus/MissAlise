using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Interfaces;

namespace MissAlise.Application.Providers
{
	public interface IAsyncHandlersProvider
	{
		IAsyncHandler<TCommand> GetHandler<TCommand>();
	}

	internal class AsyncHandlersProvider : IAsyncHandlersProvider
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
