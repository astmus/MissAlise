using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MissAlise.Application.Context;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Providers;
using MissAlise.Application.UseCases.Sync;
using MissAlise.Entities.OneDrive;

namespace MissAlise.Application
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.TryAddScoped<IHandleContext, HandleContext>();
			services.TryAddScoped<IContextItems, ContextItems>();
			//services.TryAddTransient(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>(throwIfNull: false));
			
			return services;
		}

		public static IServiceCollection AddOneDriveHandling(this IServiceCollection services)
		{
			services.AddScoped<IAsyncHandlersProvider, AsyncHandlersProvider>()
									.AddScoped<IAsyncHandler<SyncCommand>, SyncCommandHandler>();
			return services;
		}
	}
}
