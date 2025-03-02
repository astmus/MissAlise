using Microsoft.Extensions.Logging;
using MissAlise.Background;

namespace MissAlise.Application.Background.Handlers
{
	public class SyncOneDriveJobHandler : BackgroundJobHandler<SyncOneDriveJob>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> logger;
		//private readonly GraphServiceClient graphServiceClient;		

		public SyncOneDriveJobHandler(ILogger<SyncBackgroundTaskHandler> logger/*, GraphServiceClient graphServiceClient*/)
		{
			this.logger = logger;
			//this.graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncOneDriveJob backgroundTask, CancellationToken cancel)
		{
			logger.LogInformation("Start task {task}", nameof(SyncOneDriveJobHandler));
			for (var i = 0; i < 5; i++)
			{
				//logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncOneDriveFolderJob));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncOneDriveJob> job, CancellationToken cancel)
		{
			logger.LogInformation(" end task {task}", nameof(SyncOneDriveJobHandler));
			await base.EndAsync(job, cancel);
		}
	}
}