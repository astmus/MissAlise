using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#nullable disable
namespace MissAlise.Background
{
	public class BackgroundServer : BackgroundService
	{
		public static BackgroundServer Current { get; private set; }
		public Guid Id { get; init; }
		public string Name { get; set; }
		public int CurrentPressure { get => _currentPressure; private set => _currentPressure = value; }
		public int MaxPressure { get; set; } = 128;
		public int MaxBackHandlers { get => _maxBackHandlers; set { if (_maxBackHandlers < value) _workers.Release(value - _maxBackHandlers); _maxBackHandlers = value; } }
		public bool IsOverdosed => CurrentPressure > MaxPressure;

		protected readonly ILogger log;
		private readonly IEventTriggersSource triggers;

		static SemaphoreSlim _workers = new SemaphoreSlim(2, 128);
		int _currentPressure;
		int _maxBackHandlers = 2;

		public BackgroundServer(ILogger logger, IEventTriggersSource triggers) 
		{
			Current = this;
			log = logger;
			this.triggers = triggers;
		}
		
		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			do
			{
				foreach (var trigger in triggers.OrderBy(b => b.Job.LastStart ?? DateTime.MinValue).ThenBy(o => o.Job.Weight))

					if (trigger.Check())
						await trigger.Fire(cancellationToken);

				await Task.Delay(1000, cancellationToken);
			}
			while (!cancellationToken.IsCancellationRequested);
		}

		public int IncreasePressure(int pressure)
			=> Interlocked.Add(ref _currentPressure, pressure);

		public int DecreasePressure(int pressure)
			=> Interlocked.Add(ref _currentPressure, -pressure);

		public Task RentWorker(CancellationToken cancel)
			=> _workers.WaitAsync(cancel);

		public void BackWorker()
		{
			if (_workers.CurrentCount < MaxBackHandlers)
				_workers.Release();
		}
	}
}

#nullable restore