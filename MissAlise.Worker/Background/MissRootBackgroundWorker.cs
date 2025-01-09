using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MissAlise.Background;
using MissAlise.DataBase.Models;

namespace MissAlise.Worker.Background
{
	public class MissRootBackgroundWorker : BackgroundJobsRootService
	{
		private readonly IServiceScopeFactory factory;

		public MissRootBackgroundWorker(ILogger<MissRootBackgroundWorker> logger, IServiceScopeFactory factory) : base(logger)
		{
			this.factory = factory;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = factory.CreateScope();

			using var context = scope.ServiceProvider.GetRequiredService<UserMediaContext>();			
			await context.Database.MigrateAsync(cancellationToken);

			_log.LogInformation("Start service {Name}", nameof(MissRootBackgroundWorker));
			await base.StartAsync(cancellationToken);
		}
	}
}
