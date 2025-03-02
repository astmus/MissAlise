using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Common;
using MissAlise.Background;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Repositories;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddMigrationsService(this IServiceCollection services)
		{
			services.AddHostedService<MigrateService>();
			services.AddOpenTelemetry()
				.WithTracing(tracing => tracing.AddSource(MigrateService.ActivitySourceName));
			return services;
		}

		public static IServiceCollection AddPersistanceServices(this IServiceCollection services, IConfiguration appConfig)
		{
			services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>()			
						//.AddScoped<IUserProfilesRepository, UserProfilesRepository>()
						.AddScoped<IUserRepository, UserRepository>()
						.AddScoped<IPhotoRepository, PhotoRepository>()
						//.AddScoped<IUserStore<AppUser>, UserStore<AppUser>>()
						.AddScoped<IVideoRepository, VideoRepository>();
			
			services.AddIdentityCore<AppUser>(options =>
			{
				options.SignIn.RequireConfirmedAccount = false;
				options.User.RequireUniqueEmail = false;
			})
			.AddEntityFrameworkStores<IdentityContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();

			services.AddDbContext<IdentityContext>(options =>
				options.UseLazyLoadingProxies().EnableSensitiveDataLogging()
							.UseSqlite(appConfig.GetConnectionString("DefaultIdentityConnection")));
							
			var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
#if DEBUG
			settings.ServerSelectionTimeout = TimeSpan.FromSeconds(600);
			settings.ConnectTimeout = TimeSpan.FromSeconds(600); 
#endif

			var client = new MongoClient(settings);
			services.AddSingleton<IMongoClient>(client);
			services.AddSingleton<IMongoDatabase>(sp =>
			{
				var client = sp.GetRequiredService<IMongoClient>();
				return client.GetDatabase("postgres");
			});
			
			services.AddDbContextPool<UserMediaContext>(options =>
				options.UseNpgsql(appConfig.GetConnectionString("postgres")));
			return services;
		}
	}
	//dotnet ef migrations add InitialMigration --context UserMediaContext --output-dir Migrations
}
