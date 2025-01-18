using MissAlise.Background;

namespace MissAlise.Worker.Background.Handlers
{
	public class SyncBackgroundTaskHandler : BackgroundJobHandler<SyncDataJob>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> logger;
		private readonly IServiceScopeFactory factory;		

		public SyncBackgroundTaskHandler(ILogger<SyncBackgroundTaskHandler> logger/*, GraphServiceClient graphServiceClient*/, IServiceScopeFactory factory)
		{
			this.logger = logger;
			this.factory = factory;
			//_graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncDataJob backgroundTask, CancellationToken cancel)
		{
			//var drive = await _graphServiceClient.Me.Drive.GetAsync();
			//var root = await _graphServiceClient.Me.Drive.Root.Request().GetAsync();
			logger.LogInformation("Start task {task}", nameof(SyncDataJob));
			for (var i = 0; i < 5; i++)
			{
				//logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncDataJob));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncDataJob> job, CancellationToken cancel)
		{
			logger.LogInformation("end task {task}", nameof(SyncDataJob));
			await base.EndAsync(job, cancel);
		}
	}
}