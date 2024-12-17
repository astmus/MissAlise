using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MissAlise.Background
{
#nullable disable
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
				var backJob = handleScope.ServiceProvider.GetService<IOptions<BackgroundJob<TJob>>> ().Value;
				var dataCollection = handleScope.ServiceProvider.GetService<JobDataCollection<TJob>>();
				var trigger = handleScope.ServiceProvider.GetService<IOptions<EventTrigger<TJob>>>().Value;

				foreach (var data in dataCollection)
				{
					var job = backJob with
					{
						JobData = data,
						Description = trigger.Description + " " + trigger.ToString()[15..^2].Split(",").Where(w => w.EndsWith("= ") == false).Skip(2).Aggregate((s, S2k) => s + S2k) 
					};

					trigger.SetJob(job);
					EventTriggerCollection.Instance.Add(trigger with { FireStarter = _jobsChannel.Writer.WriteAsync });
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

					job.Start();

					await handler.StartAsync(job, cancel.Token).ConfigureAwait(false);
					await handler.HandleJobAsync(job, cancel.Token).ConfigureAwait(false);
					await handler.EndAsync(job, cancel.Token).ConfigureAwait(false);

					job.End(JobCompletionState.Success);
					log.LogInformation("Обработка успешна {job}", job);
				}
			}
			catch (OperationCanceledException)
			{
				job.End(JobCompletionState.Cancelled);
				log.LogWarning("Отмена фоновой задачи: {job}", job);
			}
			catch (Exception error)
			{
				job.End(JobCompletionState.Failed);
				log.LogError("Ошибка фоновой задачи: {job} {error}",job, error);
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
