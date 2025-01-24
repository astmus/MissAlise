using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var postgreServer = builder.AddPostgres("pgsrv").WithLifetime(ContainerLifetime.Persistent).WithHttpEndpoint(5432,5432);
var db = postgreServer.AddDatabase("missdb");

var api = builder.AddProject<Projects.MissAlise_WebApi>("webapi").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);
builder.AddProject<Projects.MissAlise_Worker>("worker").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);
//builder.AddProject<Projects.MissAlise_BotService>("bot");
//builder.AddProject<Projects.MissAlise_Bot>("bot").WithExternalHttpEndpoints().WithReference(db).WaitFor(db).WithReference(api).WaitFor(api);
//builder.AddProject<Projects.WebBotService>("bot").WaitFor(db);

builder.Build().Run();
