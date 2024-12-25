
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
		builder => builder.SetDescription("���������� �������������").SetWeight(10).AddTrigger(new UpdateUsersBackgroundTask(64), "����������").SetDelay(Time.Minute)		
	);

//	AddBackgroundService<UpdateIssuesBackgroundTask, UpdateIssuesJobHandler>().AddTrigger<UpdateIssuesBackgroundTask>(
//		t => { t.Delay = TimeSpan.FromMinutes(30); t.Description = "������������� �����"; })
//	.AddBackgroundService<SyncBackgroundTask, SyncJobHandler>().AddTrigger<SyncBackgroundTask>(
//		t => { t.Delay = TimeSpan.FromDays(1); t.Description = "������������� ������"; });
builder.Build().Run();
