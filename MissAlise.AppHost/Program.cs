var builder = DistributedApplication.CreateBuilder(args);

var postgreServer = 
builder.AddPostgres("psserver").WithLifetime(ContainerLifetime.Persistent);
var db = postgreServer.AddDatabase("missdb");

var api = builder.AddProject<Projects.MissAlise_WebApi>("alise-webapi").WithReference(db).WaitFor(db);
builder.AddProject<Projects.MissAlise_Core>("alise-core").WithReference(db).WithReference(api).WaitFor(db);

builder.Build().Run();
