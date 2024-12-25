using Microsoft.Extensions.DependencyInjection;
using MissAlise.Background;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddPersistanceService(this IServiceCollection services)
		{
			services.AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017"));
			services.AddSingleton<IMongoDatabase>(sp =>
			{
				var client = sp.GetRequiredService<IMongoClient>();
				return client.GetDatabase("Mongo"); // Название вашей базы данных
			});
			services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
			return services;
		}
	}
}
