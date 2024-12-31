using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using MissAlise.WebApi.OneDrive;
using Process = System.Diagnostics.Process;

namespace MissAlise.WebApi.Auth
{
	public class DelegateAuthenticationProvider2 : IAuthenticationProvider
	{
		private readonly AzureConfiguration config;
		static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
		};

		public DelegateAuthenticationProvider2(IOptions<AzureConfiguration> options)
		{
			this.config = options.Value;
		}

		public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
		{
			var client = new HttpClient();

			//var url = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id={config.ClientId}&scope={config.Scopes}&response_type=code&redirect_uri={config.RedirectUri}";
			var cca = ConfidentialClientApplicationBuilder.Create(config.ClientId)
					.WithClientSecret(config.ClientSecret)
					.WithAuthority(new Uri($"https://login.microsoftonline.com/common/oauth2/v2.0/authorize"))
					.WithRedirectUri(config.RedirectUri)
					.Build();

			var url = await cca.GetAuthorizationRequestUrl(config.GetScopes()).ExecuteAsync();
			try
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = url.ToString(),
						UseShellExecute = true
					});
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url.ToString());
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url.ToString());
				}
				else
				{
					Console.WriteLine("Operating system is not supported");
				}
				var authorizationCode = GetAuthorizationCode(config.RedirectUri);
				//var scopes = config.GetScopes();
				//var ack = await cca.AcquireTokenByAuthorizationCode("offline_access+user.read+files.readwrite+openid+profile".Split('+'), authorizationCode).ExecuteAsync();
				
				
				var tokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
				
				var content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("client_id", config.ClientId),
					new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
					new KeyValuePair<string, string>("client_secret", config.ClientSecret),
					new KeyValuePair<string, string>("code", authorizationCode),
					new KeyValuePair<string, string>("grant_type", "authorization_code")
				});

				// Отправка POST-запроса
				var response = await client.PostAsync(tokenUrl, content);
				AuthenticationResponse tokenResponse = null;


				// Проверка результата
				if (response.IsSuccessStatusCode)
				{
					var str = await response.Content.ReadAsStringAsync();					
					tokenResponse = JsonSerializer.Deserialize<AuthenticationResponse>(str, _options);
					Console.WriteLine("Response: " + tokenResponse);
				}
				else
				{
					Console.WriteLine("Error: " + response.StatusCode);
				}

				request.Headers.Add("Authorization", $"Bearer {tokenResponse?.AccessToken}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Не удалось открыть ссылку: {ex.Message}");
			}
		}

		static string GetAuthorizationCode(string redirectUri)
		{
			// Создаём HttpListener
			var listener = new HttpListener();
			listener.Prefixes.Add(redirectUri);
			listener.Start();

			Console.WriteLine($"Ожидаем запрос на {redirectUri}...");

			var context = listener.GetContext();
			var query = context.Request.QueryString;

			string authorizationCode = query["code"];
			if (string.IsNullOrEmpty(authorizationCode))
			{
				var error = query.Get("error");
				var errorDescription = query.Get("error_description");
				throw new Exception(error + errorDescription);
			}

			var response = context.Response;
			string responseString = "<html><body>Authorization code obtained. </body></html>";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			response.OutputStream.Write(buffer, 0, buffer.Length);
			response.OutputStream.Close();
			response.Close();

			listener.Stop();
			return authorizationCode;
		}
	}
}
