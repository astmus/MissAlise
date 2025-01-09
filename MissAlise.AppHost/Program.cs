using System.Globalization;
using Aspire.Hosting;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sqlserver = builder.AddPostgres("psserver")
	.WithLifetime(ContainerLifetime.Persistent);

var db = sqlserver.AddDatabase("missdb");

var backWorker = builder.AddProject<Projects.MissAlise_Worker>("missalise-worker")
	.WithReference(db)
	.WaitFor(db);

builder.AddProject<Projects.MissAlise_WebApi>("missalise-webapi")
	.WithReference(db)
	.WaitFor(backWorker);

//builder.Environment.IsDevelopment();
builder.Build().Run();
