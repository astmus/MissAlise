using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Common;
using MissAlise.Background;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Models;
using MissAlise.DataBase.Repositories;
using MissAlise.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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

			// MongoDB.Driver 3.2+: GuidRepresentation настраивается через глобальную регистрацию сериализатора
			BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

			var mongoCS = appConfig.GetConnectionString("missdb-mongo");
			var settings = MongoClientSettings.FromConnectionString(mongoCS);
#if DEBUG
			settings.ServerSelectionTimeout = TimeSpan.FromSeconds(600);
			settings.ConnectTimeout = TimeSpan.FromSeconds(600);
#endif

			services.AddSingleton<IMongoClient>(sp => new MongoClient(settings));
			services.AddSingleton<IMongoDatabase>(sp =>
			{
				var client = sp.GetRequiredService<IMongoClient>();
				var db = client.GetDatabase("missdb");

				var userProfiles = db.GetCollection<DbUserProfile>("UserProfiles");

				var keys = Builders<DbUserProfile>.IndexKeys
					.Ascending("Identities.Scheme")
					.Ascending("Identities.ExternalId");

				userProfiles.Indexes.CreateOne(
					new CreateIndexModel<DbUserProfile>(keys, new CreateIndexOptions
					{
						Unique = true,
						Sparse = true
					})
				);

				return db;
			});

			services.AddScoped<IUserProfilesRepository, UserProfilesRepository>();
			services.AddScoped<IAccessCredentialsStore, AccessCredentialsStore>();
			services.AddScoped<IOwnerResolver, Services.MongoOwnerResolver>();

			services.AddDbContextPool<UserMediaContext>(options =>
				options.UseNpgsql(appConfig.GetConnectionString("missdb")));
			return services;
		}
	}
}
