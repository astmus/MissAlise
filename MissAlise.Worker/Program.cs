using System.Globalization;
using MissAlise.Background;
using MissAlise.DataBase;
using MissAlise.Utils;
using MissAlise.Worker.Background;
using MissAlise.Worker.Background.Handlers;

namespace MissAlise.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{			
			var builder = Host.CreateApplicationBuilder(args);
			builder.AddServiceDefaults();

			builder.Services.AddPersistanceService();
			builder.Services.AddHostedService<BackgroundJobsRootService>();
			builder.Services
				.AddBackgroundJob<UpdateUsersJob, UpdateUsersJobHandler>(
					builder => builder.SetDescription("Обновление пользователей").SetWeight(10).AddTrigger(new UpdateUsersJob(64), "Ежеминутно").SetDelay(Time.Minute)
				).AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
					builder => builder.SetDescription("Синхронизация данных").SetWeight(10).IsDisabled().AddTrigger(new SyncDataJob(64), "Полуминутно").SetDelay(Time.Minute / 2).SetEnabled(false)
				);

			var host = builder.Build();
			host.Run();
		}
	}
}