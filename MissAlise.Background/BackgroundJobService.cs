#nullable disable
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Utils;

namespace MissAlise.Background
{
	public abstract class BackgroundJobService : BackgroundService
	{
		public static BackgroundServer Server { get; protected set; }
	}

	public class BackgroundJobService<TJob> : BackgroundJobService where TJob : class
	{
		private readonly IServiceScopeFactory _scopesFactory;
		private readonly ILogger<BackgroundJob<TJob>> log;
		Channel<BackgroundJob<TJob>> _jobsChannel;
		
		public BackgroundJobService(IServiceScopeFactory scopesFactory, ILogger<BackgroundJob<TJob>> log)
		{
			Server = new BackgroundServer() { Id = Guid.NewGuid() };	
			_scopesFactory = scopesFactory;
			this.log = log;
			_jobsChannel = Channel.CreateUnbounded<BackgroundJob<TJob>>(new UnboundedChannelOptions() { SingleWriter = true });			
		}

		public sealed override async Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				using var handleScope = _scopesFactory.CreateScope();
				var backJob = handleScope.ServiceProvider.GetRequiredService<BackgroundJob<TJob>> ();
				var jobsRepository = handleScope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>();
				var dbJob = await jobsRepository.LoadAsync<BackgroundJob<TJob>>(Id<TJob>.UniqueName, cancellationToken);
				if (dbJob.ToString() != backJob.ToString()) // сравниваем строки потому что Triggers есть указатели на List которые конечно же будут разными, хорошо бы сравнивать и эти коллекции если они изменились, но это как нить потом
					await jobsRepository.AddOrReplaceAsync(backJob, cancellationToken);
				foreach (var trigger in backJob.Triggers)
				{
					var job = backJob with {};

					trigger.Description = $"{job.Description} {trigger.Description}";
					trigger.Setup(job, _jobsChannel.Writer.WriteAsync);

					EventTriggerCollection.Instance.Add(trigger);
				}
			}
			catch (Exception e)
			{
				log.LogError("Start background service failed {error}", e);
			}

			await base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken cancel)
		{
			while (!cancel.IsCancellationRequested)

				await foreach (var job in _jobsChannel.Reader.ReadAllAsync(cancel).ConfigureAwait(false))
				{
					if (Server.IsOverdosed)
						continue;

					if (job.GetState() == JobState.ReadyToRun)
					{
						job.PendingState();
						_ = HandleAsync(job, cancel);
					}
				}
		}

		private async Task HandleAsync(BackgroundJob<TJob> job, CancellationToken hostCancel)
		{
			var handle = 0;
			try
			{
				Server.IncreasePressure(job.Weight);
				await Server.RentWorker(hostCancel);
				handle = 1;//await _masterConnector.usp_fetchhandle(job, hostCancel).ConfigureAwait(false);
				if (handle != 1) return;

				using (var handleScope = _scopesFactory.CreateScope())
				{					
					using var cancel = CancellationTokenSource.CreateLinkedTokenSource(hostCancel);

					var handler = handleScope.ServiceProvider.GetRequiredService<BackgroundJobHandler<TJob>>();
					
					log.LogInformation("Обработка {job}", job);
					job.SetCancel(cancel.Cancel);

					job.StartJob();

					await handler.StartAsync(job, cancel.Token).ConfigureAwait(false);
					await handler.HandleJobAsync(job, cancel.Token).ConfigureAwait(false);
					await handler.EndAsync(job, cancel.Token).ConfigureAwait(false);

					job.EndJob(JobCompletionState.Success);
					log.LogInformation("Обработка успешна {job}", job.Description);
				}
			}
			catch (OperationCanceledException)
			{
				job.EndJob(JobCompletionState.Cancelled);
				log.LogWarning("Отмена фоновой задачи: {job}", job.Description);
			}
			catch (Exception error)
			{
				job.EndJob(JobCompletionState.Failed);
				log.LogError("Ошибка фоновой задачи: {job} {error}", job.Description, error);
			}
			finally
			{
				if (handle != 0)
				{
					var result = 1;// await _masterConnector.usp_completehandle(job, hostCancel).ConfigureAwait(false);
					if (result != 1)
						log.LogWarning("Завершение обработки завершилось с результатом {result}", result);
				}
				job.ResetState();
				Server.DecreasePressure(job.Weight);
				Server.BackWorker();
			}
		}
	}
#nullable enable
}
