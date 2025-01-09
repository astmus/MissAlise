using MissAlise.Background;
using MissAlise.Bot;
using MissAlise.DataBase;
using MissAlise.Utils;
using MissAlise.Worker.Background;
using MissAlise.Worker.Background.Handlers;

namespace MissAlise.Core
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);
			builder.AddServiceDefaults();

			builder.Services.AddPersistanceService(builder.Configuration);
			builder.Services.AddBackgroundServer<MissAliseBackgroundServer>();
			builder.Services.AddBackgroundJob<UpdateUsersJob, UpdateUsersJobHandler>(
					builder => builder.SetDescription("Обновление пользователей").AddTrigger(new UpdateUsersJob(64), "Ежеминутно").SetDelay(Time.Minute)
				);
			builder.Services.AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
					builder => builder.SetDescription("Синхронизация данных").AddTrigger(new SyncDataJob(64), "Полуминутно").SetDelay(Time.Minute / 2)
				);
			builder.Services.AddPresentationBot(builder.Configuration);
			builder.Build().Run();
		}
	}
}
