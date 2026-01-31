using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

// Parameters
var pgUser = builder.AddParameter("pgUser", "postgres");
var pgPass = builder.AddParameter("pgPass", "postgres");

// Redis
var cache = builder.AddRedis("cache", 6606);

// PostgreSQL
var postgres = builder.AddPostgres("psserver", pgUser, pgPass)
	.WithEndpoint(
		name: "postgresendpoint",
		scheme: "tcp",
		port: 5432,
		targetPort: 5432,
		isProxied: false)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithImageTag("17.0")
	.WithEnvironment(env =>
	{
		env.EnvironmentVariables["POSTGRES_DB"] = "missdb";
		env.EnvironmentVariables["PGDATA"] = "/var/lib/postgresql/data/pgdata";
	})
	.WithVolume("psserver-data", "/var/lib/postgresql/data");

var pgDb = postgres.AddDatabase("missdb");

var mongoUser = builder.AddParameter("mongoUser", "admin");
var mongoPass = builder.AddParameter("mongoPass", "ChangeMe123!");

var mongo = builder.AddMongoDB("mongo", port: 27017, mongoUser, mongoPass)
	.WithImage("futark/mongo-seeded")
	.WithImageTag("dump-v1")	
	.WithDataVolume("mongo-seeded-data")
	.WithLifetime(ContainerLifetime.Persistent);

var mongoDb = mongo.AddDatabase("missdb-mongo", databaseName: "missdb");

// WebApi
builder.AddProject<Projects.MissAlise_WebApi>("webapi")	
	.WithExternalHttpEndpoints()
	.WithReference(cache)
	.WithReference(pgDb)
	.WithReference(mongoDb)
	.WaitFor(cache)
	.WaitFor(pgDb)
	.WaitFor(mongoDb)
	;

//builder.AddProject<Projects.>("bot")
//    .WithReference(cache)
//    .WithReference(pgDb)
//    .WithReference(mongoDb)
//    .WaitFor(cache)
//    .WaitFor(pgDb)
//    .WaitFor(mongoDb);

builder.Build().Run();