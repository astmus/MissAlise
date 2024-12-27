using Microsoft.Extensions.DependencyInjection;
using MissAlise.AppHost.Background;
using MissAlise.AppHost.Background.Handlers;
using MissAlise.Background;
using MissAlise.DataBase;
using MissAlise.Utils;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MissAlise_WebApi>("missalise-webapi");
builder.Services.AddPersistanceService();

builder.Services.AddHostedService<BackgroundJobsRootService>();
builder.Services
	.AddBackgroundJob<UpdateUsersBackgroundTask, UpdateUsersJobHandler>(
		builder => builder.SetDescription("Обновление пользователей").SetWeight(10).AddTrigger(new UpdateUsersBackgroundTask(64), "Ежеминутно").SetDelay(Time.Minute)		
	).AddBackgroundJob<SyncBackgroundTask, SyncBackgroundTaskHandler>(
		builder => builder.SetDescription("Синхронизация данных").SetWeight(10).IsDisabled().AddTrigger(new SyncBackgroundTask(64), "Полуминутно").SetDelay(Time.Minute/2).SetEnabled(false)
	);

builder.Build().Run();
