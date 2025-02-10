using Microsoft.Graph;
using MissAlise.Background;

namespace MissAlise.Worker.Background.Handlers
{
	public class SyncOneDriveFolderJobHandler : BackgroundJobHandler<SyncOneDriveFolderJob>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> logger;
		private readonly GraphServiceClient graphServiceClient;		

		public SyncOneDriveFolderJobHandler(ILogger<SyncBackgroundTaskHandler> logger/*, GraphServiceClient graphServiceClient*/)
		{
			this.logger = logger;
			//this.graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncOneDriveFolderJob backgroundTask, CancellationToken cancel)
		{
			logger.LogInformation("Start task {task}", nameof(SyncOneDriveFolderJobHandler));
			for (var i = 0; i < 5; i++)
			{
				//logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncOneDriveFolderJob));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncOneDriveFolderJob> job, CancellationToken cancel)
		{
			logger.LogInformation(" end task {task}", nameof(SyncOneDriveFolderJobHandler));
			await base.EndAsync(job, cancel);
		}
	}
}