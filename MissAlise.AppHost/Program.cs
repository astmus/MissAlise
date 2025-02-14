using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var ps = builder.AddPostgres("psgsql").WithLifetime(ContainerLifetime.Persistent).WithImageTag("latest");
var db = ps.AddDatabase("missdb");

var api = builder.AddProject<Projects.MissAlise_WebApi>("webapi").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);
//builder.AddProject<Projects.MissAlise_Worker>("worker").WithExternalHttpEndpoints().WithReference(db);
builder.AddProject<Projects.MissAlise_PhotoService>("photo").WithHttpsEndpoint(port:7216).WithHttpEndpoint(7218).WithExternalHttpEndpoints().WithReference(db).WithReference(cache).WaitFor(cache);
builder.AddProject<Projects.MissAlise_VideoService>("video").WithHttpsEndpoint(port: 7220).WithHttpEndpoint(7222).WithExternalHttpEndpoints().WithReference(db).WithReference(cache).WaitFor(cache);
//builder.AddProject<Projects.MissAlise_BotService>("bot");
//builder.AddProject<Projects.MissAlise_Bot>("bot").WithExternalHttpEndpoints().WithReference(db).WaitFor(db).WithReference(api).WaitFor(api);
//builder.AddProject<Projects.WebBotService>("bot").WaitFor(db);

builder.Build().Run();
