using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace MissAlise.Background
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBackgroundJob<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TJob, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>
			(this IServiceCollection services, 
			Func<BackgroundJob.Builder, BackgroundJob.Builder> backConfig, 
			Func<EventTrigger.Builder, EventTrigger.Builder> triggerConfig,
			params TJob[] jobs) where TJob : class where THandler : BackgroundJobHandler<TJob>
		{
			services.AddHostedService<BackgroundJobService<TJob>>()
						 .AddTransient<BackgroundJobHandler<TJob>, THandler>()
						 .AddScoped<BackgroundJob<TJob>>()
						 .AddScoped<EventTrigger<TJob>>()
						 .AddScoped<JobDataCollection<TJob>>(sp=>
						 {
							 var collection = new JobDataCollection<TJob>();
							 collection.AddRange(jobs);
							 return collection;
						 });

			return services
						.ConfigureAll<BackgroundJob<TJob>>(b => backConfig(new BackgroundJob.Builder(b)))
						.ConfigureAll<EventTrigger<TJob>>(t => triggerConfig(new EventTrigger.Builder(t)));
		}
	}
}
