using System.Diagnostics;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Contexts;

namespace MissAlise.DataBase
{
	internal class MigrateService : BackgroundService
	{
		private readonly IServiceScopeFactory factory;
		private readonly ILogger<MigrateService> log;
		internal const string ActivitySourceName = "Migrations";
		private static readonly ActivitySource activitySource = new(ActivitySourceName);

		public MigrateService(IServiceScopeFactory factory, ILogger<MigrateService> log)
		{
			this.factory = factory;
			this.log = log;
		}
		public override async Task StartAsync(CancellationToken cancel)
		{
			using var activity = activitySource.StartActivity("Migrating database", ActivityKind.Client);
			try
			{
				using var scope = factory.CreateScope();
				using var context = scope.ServiceProvider.GetRequiredService<UserMediaContext>();
				await context.Database.MigrateAsync(cancel);
				LinqToDBForEFTools.Initialize();
			}
			catch (Exception error)
			{
				activity?.SetStatus(ActivityStatusCode.Error);
				activity?.SetTag("error.message", error.Message);
				log.LogError(error, "Database migration error {message}", error.Message);
				throw;
			}
		}

		protected override Task ExecuteAsync(CancellationToken cancel)
			=> Task.CompletedTask;
	}
}
