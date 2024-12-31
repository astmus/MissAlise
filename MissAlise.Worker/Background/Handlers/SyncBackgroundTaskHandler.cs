using Microsoft.Extensions.Logging;
using MissAlise.Background;
using MissAlise.Utils;
using MissAlise.Worker.Background;

namespace MissAlise.Worker.Background.Handlers
{
	public class SyncBackgroundTaskHandler : BackgroundJobHandler<SyncDataJob>
	{
		private readonly ILogger<SyncBackgroundTaskHandler> _logger;
		//private readonly GraphServiceClient _graphServiceClient;

		public SyncBackgroundTaskHandler(ILogger<SyncBackgroundTaskHandler> logger/*, GraphServiceClient graphServiceClient*/)
		{
			_logger = logger;
			//_graphServiceClient = graphServiceClient;
		}

		public override async Task HandleAsync(SyncDataJob backgroundTask, CancellationToken cancel)
		{
			//var drive = await _graphServiceClient.Me.Drive.GetAsync();
			//var root = await _graphServiceClient.Me.Drive.Root.Request().GetAsync();

			for (var i = 0; i < 5; i++)
			{
				_logger.LogInformation("{i} {time} {job}", i, Time.Now, nameof(SyncDataJob));
				await Task.Delay(1000);
			}
		}

		public override async Task EndAsync(BackgroundJob<SyncDataJob> job, CancellationToken cancel)
		{
			//_logger.LogInformation(" end task {task}", nameof(SyncDataJob));
			await base.EndAsync(job, cancel);
		}
	}
}