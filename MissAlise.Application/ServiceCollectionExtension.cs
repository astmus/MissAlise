using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
			services.AddScoped<IHandleContext, HandleContext>().AddScoped<IContextItems, ContextItems>();
			services.AddScoped(sp => sp.GetRequiredService<IHandleContext>().GetCurrent<UserProfile>());
			
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
