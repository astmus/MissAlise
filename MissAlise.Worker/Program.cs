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
					builder => builder.SetDescription("���������� �������������").AddTrigger(new UpdateUsersJob(64), "����������").SetDelay(Time.Minute)
				).AddBackgroundJob<SyncDataJob, SyncBackgroundTaskHandler>(
					builder => builder.SetDescription("������������� ������").AddTrigger(new SyncDataJob(64), "�����������").SetDelay(Time.Minute / 2)
				);

			var host = builder.Build();
			host.Run();
		}
	}
}