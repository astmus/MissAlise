var builder = DistributedApplication.CreateBuilder(args);

var postgreServer = 
builder.AddPostgres("psserver").WithLifetime(ContainerLifetime.Persistent);
var db = 
postgreServer.AddDatabase("missdb");
var backWorker = 
builder.AddProject<Projects.MissAlise_Worker>("missalise-worker").WithReference(db).WaitFor(db);
builder.AddProject<Projects.MissAlise_WebApi>("missalise-webapi").WithReference(db).WaitFor(backWorker);

builder.Build().Run();
