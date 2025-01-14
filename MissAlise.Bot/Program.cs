//using System.Text.Json;
//using Microsoft.AspNetCore.Mvc;

//namespace MissAlise.Bot
//{
//	public class Program
//	{
//		public static void Main(string[] args)
//		{
//			var builder = WebApplication.CreateBuilder(args);

//			// Add service defaults & Aspire client integrations.
//			builder.AddServiceDefaults(false);

//			// Add services to the container.
//			builder.Services.AddProblemDetails();

//			builder.Services
//				.AddSingleton<Bot>().Configure<BotConfig>(builder.Configuration.GetSection(nameof(BotConfig)))
//				.AddTransient<AzureAd>(sp 
//					=> sp.GetRequiredService<IConfiguration>().GetSection(nameof(AzureAd)).Get<AzureAd>())
//				.AddHostedService<Bot.UpdateReceiver>()
//				.AddHostedService<Bot.UpdateHandler>()
//				.AddHttpClient("bot")
//				.UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2)) // Recreate connection every 2 minutes
//				.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

//			var app = builder.Build();

//			app.MapGet("/signin-oidc", Signin);

//			app.Run();
//		}

//		static async Task<IResult> Signin([FromServices] AzureAd config, [FromServices] HttpClient client, [FromQuery] string code, [FromQuery] int state)
//		{
//			var content = new FormUrlEncodedContent(new[]
//					{

//					new KeyValuePair<string, string>("client_id", config.ClientId),
//					new KeyValuePair<string, string>("redirect_uri", config.RedirectUri+config.CallbackPath),
//					new KeyValuePair<string, string>("client_secret", config.ClientSecret),
//					new KeyValuePair<string, string>("code", code),
//					new KeyValuePair<string, string>("grant_type", "authorization_code")
//				});

//			var response = await client.PostAsync(config.TokenLink, content);

//			if (response.IsSuccessStatusCode)
//			{
//				var str = await response.Content.ReadAsStringAsync();
//				var tokenResponse = JsonSerializer.Deserialize<AuthenticationResponse>(str, _options);
//				Console.WriteLine("Response: " + tokenResponse);
//			}
//			else
//				Console.WriteLine("Error: " + response.StatusCode);
//			return TypedResults.Ok();
//		}

//		static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
//		{
//			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
//			DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
//		};
//	}
//}