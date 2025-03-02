using Microsoft.Extensions.Logging;
namespace MissAlise.Background
{
#nullable disable
	public abstract class BackgroundJobHandler<TJobTask> where TJobTask : class
	{
		protected ILogger log { get; set; }

		public virtual Task StartAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		public abstract Task HandleAsync(TJobTask backgroundTask, CancellationToken cancel);

		public virtual Task EndAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		public virtual Task HandleJobAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return HandleAsync(job.Data, cancel);
		}
	}
#nullable enable
}
