using Microsoft.Extensions.Logging;
using MissAlise.Background;
using MissAlise.Utils;

namespace MissAlise.AppHost.Background.Handlers
{
	public class SyncBackgroundTaskHandler : BackgroundJobHandler<SyncBackgroundTask>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> _logger;
		//private readonly GraphServiceClient _graphServiceClient;

		public SyncBackgroundTaskHandler(ILogger<SyncBackgroundTaskHandler> logger/*, GraphServiceClient graphServiceClient*/)
		{
			_logger = logger;
			//_graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncBackgroundTask backgroundTask, CancellationToken cancel)
		{
			//var drive = await _graphServiceClient.Me.Drive.GetAsync();
			//var root = await _graphServiceClient.Me.Drive.Root.Request().GetAsync();

			for (int i = 0; i < 10; i++)
			{
				_logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncBackgroundTask));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncBackgroundTask> job, CancellationToken cancel)
		{
			_logger.LogInformation(" end task {task}", nameof(SyncBackgroundTask));
			await base.EndAsync(job, cancel);
		}
	}
}