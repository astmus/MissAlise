using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Worker.Background;
using MissAlise.Worker.Background.Handlers;

namespace MissAlise.Worker
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddBackgroundWorker(this IServiceCollection services, IConfiguration appConfig)
		{
			//services.AddPersistanceService(appConfig);

			//services.AddBackgroundServer<MissAliseBackgroundServer>();
			//services
			//	.AddBackgroundJob<UpdateUsersJob, UpdateUsersJobHandler>(
			//		builder => builder.SetDescription("Обновление пользователей").AddTrigger(new UpdateUsersJob(64), "Ежеминутно").SetDelay(Time.Minute)
			//	).AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
			//		builder => builder.SetDescription("Синхронизация данных").AddTrigger(new SyncDataJob(64), "Полуминутно").SetDelay(Time.Minute / 2)
			//	);
			return services;
		}
	}
}
