using Microsoft.Extensions.Logging;
using MissAlise.Background;
using MissAlise.Utils;

namespace MissAlise.AppHost.Background.Handlers
{
	public class UpdateUsersJobHandler : BackgroundJobHandler<UpdateUsersBackgroundTask>
	{
		private readonly ILogger<UpdateUsersJobHandler> logger;

		public UpdateUsersJobHandler(ILogger<UpdateUsersJobHandler> logger)
		{
			this.logger = logger;
		}

		public override async Task HandleAsync(UpdateUsersBackgroundTask backgroundTask, CancellationToken cancel)
		{
			for (int i = 0; i < 10; i++)
			{
				logger.LogInformation("{i} {time}", i, Time.Now);
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<UpdateUsersBackgroundTask> job, CancellationToken cancel)
		{
			logger.LogInformation(" end task {task}", nameof(UpdateUsersJobHandler));
			await base.EndAsync(job, cancel);
		}
	}
}