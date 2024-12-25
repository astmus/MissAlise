using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
namespace MissAlise.Background
{
#nullable disable
	public abstract class BackgroundJobHandler<TJobTask> where TJobTask : class
	{
		internal ILogger _logger { get; set; }

		//static int HandlersCount = 0;
		protected BackgroundJobHandler()
		{

		}
		public virtual Task StartAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		public abstract Task HandleAsync(TJobTask backgroundTask, CancellationToken cancel);

		public virtual Task EndAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		public Task HandleJobAsync(BackgroundJob<TJobTask> job, CancellationToken cancel)
		{
			return HandleAsync(job.Data, cancel);
		}

		protected void LogError(string message, Exception error = default, [CallerMemberName] string method = default)
		{
			_logger.LogError("{method} {error}", method, error);
		}

		protected void LogInfo(string message, [CallerMemberName] string method = default)
		{
			_logger.LogInformation("{method}, {message}", method, message);
		}
	}	
#nullable enable
}
