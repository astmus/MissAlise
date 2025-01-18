using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Models;

namespace MissAlise.DataBase
{
	internal class MigrateService : BackgroundService
	{
		private readonly IServiceScopeFactory factory;
		private readonly ILogger<MigrateService> log;

		public MigrateService(IServiceScopeFactory factory, ILogger<MigrateService> log)
		{
			this.factory = factory;
			this.log = log;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				using var scope = factory.CreateScope();

				using var context = scope.ServiceProvider.GetRequiredService<UserMediaContext>();
				await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception error)
			{
				log.LogError(error, "DAtabase migration error {message}", error.Message);
				throw;
			}
			await base.StartAsync(cancellationToken);
		}
		protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
	}
}
