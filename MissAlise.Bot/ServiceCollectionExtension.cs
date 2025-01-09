using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MissAlise.Bot
{
	public static class ServiceCollectionExtension
	{
		public static IServiceCollection AddPresentationBot(this IServiceCollection services, IConfiguration appConfig)
		{
			services
				.AddSingleton<Bot>().Configure<BotConfig>(appConfig.GetSection(nameof(BotConfig)))
				.AddHostedService<Bot.UpdateReceiver>()
				.AddHostedService<Bot.UpdateHandler>()
				.AddHttpClient<Bot.UpdatesChannel>().ConfigureHttpClient(client =>
				{
					client.Timeout = TimeSpan.FromMinutes(5); // Бесконечный тайм-аут на стороне HttpClient (важно для Long Polling)
				}).AddStandardResilienceHandler(config =>
				{
					TimeSpan timeSpan = TimeSpan.FromMinutes(3);
					config.AttemptTimeout.Timeout = timeSpan;
					config.CircuitBreaker.SamplingDuration = timeSpan * 2;
					config.TotalRequestTimeout.Timeout = timeSpan * 3;
				}); ;
				//.AddPolicyHandler(GetLongPollingPolicy())
				//.UseSocketsHttpHandler(soket => soket.Configure((handler, sp) => handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(3)));
			//.ConfigureHttpClient(http=>http.Timeout = TimeSpan.FromMinutes(8)).SetHandlerLifetime(TimeSpan.FromMinutes(8))
			//.AddTransientHttpErrorPolicy(policyBuilder =>
			//																																policyBuilder.WaitAndRetryAsync(3, retryNumber => TimeSpan.FromMilliseconds(600)));
			return services;
		}
		//static IAsyncPolicy<HttpResponseMessage> GetLongPollingPolicy()
		//{
		//	// Политика для обработки ошибок и повторных попыток
		//	return HttpPolicyExtensions
		//		.HandleTransientHttpError() // HTTP 5xx, 408
		//		.Or<TaskCanceledException>() // Учитываем отмену запроса (важно для тайм-аутов)
		//		.WaitAndRetryAsync(3, // Количество попыток
		//			retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Экспоненциальная задержка
		//			(outcome, timespan, retryCount, context) =>
		//			{
		//				Console.WriteLine($"Повтор #{retryCount} через {timespan.TotalSeconds} секунд из-за {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}.");
		//			});
		//}
	}
}
