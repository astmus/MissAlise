using Microsoft.Graph;
using MissAlise.Background;
using MissAlise.Utils;

namespace MissAlise.Worker.Background.Handlers
{
	public class SyncOneDriveFolderJobHandler : BackgroundJobHandler<SyncOneDriveFolderJob>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> logger;
		private readonly GraphServiceClient graphServiceClient;
		private readonly IServiceScopeFactory factory;		

		public SyncOneDriveFolderJobHandler(ILogger<SyncBackgroundTaskHandler> logger, GraphServiceClient graphServiceClient)
		{
			this.logger = logger;
			this.graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncOneDriveFolderJob backgroundTask, CancellationToken cancel)
		{
			for (var i = 0; i < 5; i++)
			{
				logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncOneDriveFolderJob));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncOneDriveFolderJob> job, CancellationToken cancel)
		{
			//_logger.LogInformation(" end task {task}", nameof(SyncDataJob));
			await base.EndAsync(job, cancel);
		}
	}
}