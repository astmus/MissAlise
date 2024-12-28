using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MissAlise.Background
{
	public sealed class BackgroundJobsRootService : BackgroundService
	{
		private readonly ILogger _log;

		public BackgroundJobsRootService(ILogger<BackgroundJobsRootService> logger)
		{
			_log = logger;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_log.LogInformation("Start service");
			return base.StartAsync(cancellationToken);
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_log.LogInformation("Start service");
			return base.StopAsync(cancellationToken);
		}
		
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			do
			{
				foreach (var trigger in EventTriggerCollection.Instance.OrderBy(b => b.Job.LastStart ?? DateTime.MinValue).ThenBy(o => o.Job.Weight))
					if (trigger.Check())
						await trigger.Fire(cancellationToken);

				await Task.Delay(1000, cancellationToken);
			}
			while (!cancellationToken.IsCancellationRequested);
		}
	}
}
