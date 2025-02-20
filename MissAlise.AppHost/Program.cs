using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var db = builder.AddPostgres("psgsql").WithLifetime(ContainerLifetime.Persistent).WithImageTag("latest").AddDatabase("missdb");

builder.AddProject<Projects.MissAlise_WebApi>("webapi").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);
builder.AddProject<Projects.MissAlise_PhotoService>("photo").WithHttpsEndpoint(port:7216).WithHttpEndpoint(7218).WithExternalHttpEndpoints().WithReference(cache).WaitFor(cache).WithReference(db).WaitFor(db);
builder.AddProject<Projects.MissAlise_VideoService>("video").WithHttpsEndpoint(port: 7220).WithHttpEndpoint(7222).WithExternalHttpEndpoints().WithReference(cache).WaitFor(cache).WithReference(db).WaitFor(db);

//builder.AddProject<Projects.MissAlise_Worker>("worker").WithExternalHttpEndpoints().WithReference(db);
//builder.AddProject<Projects.MissAlise_BotService>("bot");
//builder.AddProject<Projects.MissAlise_Bot>("bot").WithExternalHttpEndpoints().WithReference(db).WaitFor(db).WithReference(api).WaitFor(api);
//builder.AddProject<Projects.WebBotService>("bot").WaitFor(db);
builder.Build().Run();
