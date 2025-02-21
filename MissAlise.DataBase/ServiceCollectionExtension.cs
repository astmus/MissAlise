using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Models;
using MissAlise.Background;
using MissAlise.DataBase.Contexts;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddMigrateService(this IServiceCollection services)
		{
			services.AddHostedService<MigrateService>();
			services.AddOpenTelemetry()
				.WithTracing(tracing => tracing.AddSource(MigrateService.ActivitySourceName));
			return services;
		}

		public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration appConfig)
		{
			services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
			services.AddScoped<IUserProfilesRepository, UserProfilesRepository>()
						.AddScoped<IUserRepository, UserRepository>()
						.AddScoped<IPhotoRepository, PhotoRepository>()
			//			.AddScoped<IUserStore<AppUser>, UserStore<AppUser>>()
						.AddScoped<IVideoRepository, VideoRepository>();
			
			services.AddIdentityCore<AppUser>().AddEntityFrameworkStores<IdentityContext>();

			services.AddDbContext<IdentityContext>(options =>
				options.UseSqlite(appConfig.GetConnectionString("DefaultIdentityConnection")));

			services.AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017"));
			services.AddSingleton<IMongoDatabase>(sp =>
			{
				var client = sp.GetRequiredService<IMongoClient>();
				return client.GetDatabase("missdb");
			});
			
			services.AddDbContextPool<UserMediaContext>(options =>
				options.UseNpgsql(appConfig.GetConnectionString("missdb")));
			return services;
		}
	}
}
