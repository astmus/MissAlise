#nullable disable
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MissAlise.Utils;

namespace MissAlise.Background
{
	public class BackgroundJobService<TJob> : BackgroundService where TJob : class
	{
		BackgroundServer Server => BackgroundServer.Current;
		private readonly IServiceScopeFactory scopesFactory;
		private readonly ILogger<TJob> log;
		private readonly IEventTriggersSource triggers;
		private Channel<BackgroundJob<TJob>> jobsChannel;

		public BackgroundJobService(IServiceScopeFactory scopesFactory, ILogger<TJob> log, IEventTriggersSource triggers)
		{
			this.scopesFactory = scopesFactory;
			this.log = log;
			this.triggers = triggers;
			jobsChannel = Channel.CreateUnbounded<BackgroundJob<TJob>>(new UnboundedChannelOptions() { SingleWriter = true });
		}

		public sealed override async Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				using var handleScope = scopesFactory.CreateScope();
				var backJob = handleScope.ServiceProvider.GetRequiredService<BackgroundJob<TJob>>();
				var jobsRepository = handleScope.ServiceProvider.GetRequiredService<IBackgroundJobRepository>();
				var dbJob = await jobsRepository.LoadAsync<BackgroundJob<TJob>>(Identity<TJob>.Discriminator, cancellationToken);

				if (dbJob?.ToString() != backJob.ToString()) // сравниваем строки потому что Triggers есть указатели на List которые конечно же будут разными, хорошо бы сравнивать и эти коллекции если они изменились, но это как нить потом
					await jobsRepository.AddOrReplaceAsync(backJob, cancellationToken);

				BackgroundServer.Current.AddJob(backJob);
				foreach (var trigger in backJob.Triggers)
				{
					var job = backJob with { };
					BackgroundServer.Current.AddUserJob(job);
					trigger.Setup(job, jobsChannel.Writer.WriteAsync);
					triggers.Add(trigger with { Description = $"{job.Description} {trigger.Description}" });
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

				await foreach (var job in jobsChannel.Reader.ReadAllAsync(cancel).ConfigureAwait(false))
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
			try
			{
				Server.IncreasePressure(job.Weight);
				await Server.RentWorker(hostCancel);

				using (var handleScope = scopesFactory.CreateScope())
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
				job.ResetState();
				Server.DecreasePressure(job.Weight);
				Server.BackWorker();
			}
		}
	}
}
#nullable restore
