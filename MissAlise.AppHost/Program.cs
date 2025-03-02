using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", "postgres");
var password = builder.AddParameter("password", "postgres");

var cache = builder.AddRedis("cache", 6606);
var db = builder.AddPostgres("psserver", username, password)
	.WithEndpoint(name: "postgresendpoint", scheme: "tcp", port: 5432, targetPort: 5432, isProxied: false)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithImageTag("17.0")
	// Set the name of the default database to auto-create on container startup.
	.WithEnvironment(
	env =>
	{
		env.EnvironmentVariables.Add("POSTGRES_DB", "missdb");
		env.EnvironmentVariables.Add("PGDATA", "/var/lib/postgresql/data/pgdata");
	})
	// Mount the SQL scripts directory into the container so that the init scripts run.
	//.WithBindMount("../DatabaseContainers.ApiService/data/postgres", "/docker-entrypoint-initdb.d")
	// Configure the container to store data in a volume so that it persists across instances.
	.WithDataVolume("pgvol")
	//.WithVolume("pgvol", "/var/lib/postgresql/data/pgdata")
	// Keep the container running between app host sessions.
	.AddDatabase("postgres", "missdb")
	;

//.WithEnvironment(
//env=>
//{
//	env.EnvironmentVariables.Add("PGDATA", "/var/lib/postgresql/data/pgdata");		
//})
//.WithLifetime(ContainerLifetime.Persistent).AddDatabase("postgres");

builder.AddProject<Projects.MissAlise_WebApi>("webapi").WithExternalHttpEndpoints().WithReference(db).WaitFor(db);
//builder.AddProject<Projects.MissAlise_PhotoService>("photo").WithHttpsEndpoint(port:7216).WithHttpEndpoint(7218).WithExternalHttpEndpoints().WithReference(cache).WaitFor(cache).WithReference(db).WaitFor(db);
//builder.AddProject<Projects.MissAlise_VideoService>("video").WithHttpsEndpoint(port: 7220).WithHttpEndpoint(7222).WithExternalHttpEndpoints().WithReference(cache).WaitFor(cache).WithReference(db).WaitFor(db);
//builder.AddProject<Projects.MissAlise_Worker>("worker").WithExternalHttpEndpoints().WithReference(db);
//builder.AddProject<Projects.MissAlise_BotService>("bot");
//builder.AddProject<Projects.MissAlise_Bot>("bot").WithExternalHttpEndpoints().WithReference(db).WaitFor(db).WithReference(api).WaitFor(api);
//builder.AddProject<Projects.WebBotService>("bot").WaitFor(db);
builder.Build().Run();
