using System.Text.Json.Serialization;

namespace MissAlise.Background
{
#nullable disable
	public sealed class BackgroundServer
	{
		[JsonPropertyName("ServerId")]
		public Guid Id { get; init; }
		public string Name { get; set; }
		public int CurrentPressure { get => _currentPressure; private set => _currentPressure = value; }
		public int MaxPressure { get; set; } = 128;
		public int MaxBackHandlers { get => _maxBackHandlers; set { if (_maxBackHandlers < value) _workers.Release(value - _maxBackHandlers); _maxBackHandlers = value; } }
		public bool IsOverdosed => CurrentPressure > MaxPressure;

		static SemaphoreSlim _workers = new SemaphoreSlim(2, 128);
		int _currentPressure;
		int _maxBackHandlers = 2;


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
#nullable enable
}
