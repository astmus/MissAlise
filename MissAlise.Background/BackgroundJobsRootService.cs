using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Background.Settings;

namespace MissAlise.Background
{
	public sealed class BackgroundJobsRootService : BackgroundService
	{
		private readonly ILogger _log;

		public BackgroundJobsRootService(ILogger<BackgroundJobsRootService> logger)
		{
			_log = logger;
		}
		//public override Task StartAsync(CancellationToken cancellationToken)
		//{
		//	DatabaseNotificationService.BackgroundJobStateChanged += OnBackgroundJobStateChanged;
		//	return base.StartAsync(cancellationToken);
		//}

		//private void OnBackgroundJobStateChanged(BackgroundJobNotification notification)
		//{
		//	//if (notification.ServerId == StartupSettings.Current.ServerId) return;
		//	var state = notification;
		//	var trigger = EventTriggerCollection.Instance.FirstOrDefault(back => state.Key.EndsWith(back.JobKey) && back.Job.Organization.Id == notification.EntityId);
		//	var job = trigger?.Job;
		//	if (job == null) return;
		//	job.Completed = state.Completed;
		//	switch (state.Event)
		//	{
		//		case "start":
		//		job.LastStart = state.Occured; // time zone надо будет еще доработать сейчас выставлены так что бы были один в один на моем компе и сервере
		//		break;
		//		case "end":
		//		job.LastEnd = state.Occured; // time zone надо будет еще доработать сейчас выставлены так что бы были один в один на моем компе и сервере
		//		break;
		//		case "status":
		//		switch (state.Status)
		//		{
		//			case "stop":
		//			job.CancelJob();
		//			break;
		//			case "execute":
		//			trigger?.Fire(default);
		//			break;
		//		}
		//		break;
		//	}
		//}

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
