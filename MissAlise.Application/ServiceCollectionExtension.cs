using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MissAlise.Application.Context;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.Authentication;

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

			services.AddScoped<IAuthenticationService, AuthenticationService>();
			services.AddCascadingAuthenticationState();
			services.AddAuthentication(options =>
			{
				options.DefaultScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			});
			services.AddAuthorization();

			return services;
		}
	}
}
