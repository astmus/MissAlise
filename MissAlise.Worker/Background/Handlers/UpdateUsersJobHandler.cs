using Microsoft.Extensions.Logging;
using MissAlise.Background;
using MissAlise.Utils;
using MissAlise.Worker.Background;

namespace MissAlise.Worker.Background.Handlers
{
	public class UpdateUsersJobHandler : BackgroundJobHandler<UpdateUsersJob>
	{
		private readonly ILogger<UpdateUsersJobHandler> logger;

		public UpdateUsersJobHandler(ILogger<UpdateUsersJobHandler> logger)
		{
			this.logger = logger;
		}

		public override async Task HandleAsync(UpdateUsersJob backgroundTask, CancellationToken cancel)
		{
			for (var i = 0; i < 1; i++)
			{
				logger.LogInformation("{i} {time}", i, Time.Now);
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<UpdateUsersJob> job, CancellationToken cancel)
		{
			logger.LogInformation(" end task {task}", nameof(UpdateUsersJobHandler));
			await base.EndAsync(job, cancel);
		}
	}
}