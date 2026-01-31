using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MissAlise.Application.Context;
using MissAlise.Application.Interfaces;

namespace MissAlise.Application
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.TryAddScoped<IHandleContext, HandleContext>();
			services.TryAddScoped<IContextItems, ContextItems>();
			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssemblyContaining<ContextItems>();				
				cfg.AddBehavior<AuthBehavior>(ServiceLifetime.Scoped);
				cfg.Lifetime = ServiceLifetime.Scoped;
			});

			return services;
		}
	}
}
