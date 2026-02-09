using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MissAlise.Workflow;
using MissAlise.Workflow.Descriptors;
using StackExchange.Redis;

namespace MissAlise.TelegramBot.Workflow.State;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Регистрирует <see cref="RedisWorkflowStateStore"/> как <see cref="IWorkflowStateStore"/>.
	/// Требует зарегистрированный <see cref="IConnectionMultiplexer"/>.
	/// Сессия продлевается при каждом Save (ExpiresAt = UtcNow + DefaultSessionTtl).
	/// </summary>
	public static IServiceCollection AddRedisWorkflowStateStore(
		this IServiceCollection services,		
		Action<RedisWorkflowStateStoreOptions>? configure = null)
	{
		services.Configure<RedisWorkflowStateStoreOptions>(opt =>
		{
			configure?.Invoke(opt);
		});
		if (configure != null)
			services.Configure(configure);

		services.AddSingleton<IWorkflowStateStore>(sp =>
		{
			var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
			var options = sp.GetRequiredService<IOptions<RedisWorkflowStateStoreOptions>>().Value;
			return new RedisWorkflowStateStore(multiplexer, options);
		});

		return services;
	}
}
