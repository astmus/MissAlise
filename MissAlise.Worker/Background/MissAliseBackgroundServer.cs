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
	public class MissAliseBackgroundServer : BackgroundServer
	{
		private readonly IServiceScopeFactory factory;

		public MissAliseBackgroundServer(ILogger<MissAliseBackgroundServer> logger, IServiceScopeFactory factory, IEventTriggersSource triggers) : base(logger, triggers)
		{
			this.factory = factory;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = factory.CreateScope();

			using var context = scope.ServiceProvider.GetRequiredService<UserMediaContext>();			
			await context.Database.MigrateAsync(cancellationToken);

			log.LogInformation("Start service {Name}", nameof(MissAliseBackgroundServer));
			await base.StartAsync(cancellationToken);
		}
	}
}
