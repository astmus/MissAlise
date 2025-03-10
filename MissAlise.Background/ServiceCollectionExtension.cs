using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MissAlise.Background
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBackgroundServer<TServer>(this IServiceCollection services) where TServer : BackgroundServer
		{
			return services.AddHostedService<TServer>();
		}

		public static IServiceCollection AddBackgroundJob<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TJob,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>
		(
			this IServiceCollection services,
			Action<IBackgroundJobBuilder<TJob>> jobConfigurator
		)
		where TJob : class where THandler : BackgroundJobHandler<TJob>
		{
			services.AddHostedService<BackgroundJobService<TJob>>()
						 .AddTransient<BackgroundJobHandler<TJob>, THandler>()
						 .AddSingleton<IEventTriggersSource, EventTriggerCollection>()
						 .AddScoped(sp => sp.GetRequiredService<IOptions<BackgroundJob<TJob>>>().Value);

			services.Configure<BackgroundJob<TJob>>((job) => jobConfigurator(BackgroundJob<TJob>.CreateBuilder(job)));
			return services;
		}
	}
}