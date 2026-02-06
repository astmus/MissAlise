using MissAlise.Background;

namespace MissAlise.WebApi.Background
{
	public class MissAliseBackgroundServer : BackgroundServer
	{
		private readonly IServiceScopeFactory factory;

		public MissAliseBackgroundServer(ILogger<MissAliseBackgroundServer> logger, IServiceScopeFactory factory, IEventTriggersSource triggers) : base(logger, triggers)
		{
			this.factory = factory;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			//using var scope = factory.CreateScope();
			// здесь всякие приготовления перед стартом фоновой обработки			

			_log.LogInformation("Start service {Name}", nameof(MissAliseBackgroundServer));
			await base.StartAsync(cancellationToken);
		}
	}
}
