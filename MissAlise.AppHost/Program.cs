
using Microsoft.Extensions.DependencyInjection;
using MissAlise.AppHost.Background;
using MissAlise.AppHost.Background.Handlers;
using MissAlise.Background;
using MissAlise.Utils;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MissAlise_WebApi>("missalise-webapi");
builder.Services.AddHostedService<BackgroundJobsRootService>();

builder.Services
	.AddBackgroundJob<UpdateUsersBackgroundTask, UpdateUsersJobHandler>(
		backJob => backJob.SetDescription("Обработка сессий Db").SetWeight(10),
		trigger => trigger.SetDescription("Обработка сессий Db").SetDelay(Time.Minute),
		new UpdateUsersBackgroundTask(64), new UpdateUsersBackgroundTask(32)
		);
//	AddBackgroundService<UpdateIssuesBackgroundTask, UpdateIssuesJobHandler>().AddTrigger<UpdateIssuesBackgroundTask>(
//		t => { t.Delay = TimeSpan.FromMinutes(30); t.Description = "Синхронизация задач"; })
//	.AddBackgroundService<SyncBackgroundTask, SyncJobHandler>().AddTrigger<SyncBackgroundTask>(
//		t => { t.Delay = TimeSpan.FromDays(1); t.Description = "Синхронизация данных"; });
builder.Build().Run();
