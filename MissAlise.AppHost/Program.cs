using System.Globalization;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MissAlise_WebApi>("missalise-webapi");
builder.AddProject<Projects.MissAlise_Worker>("missalise-worker");

builder.Build().Run();
