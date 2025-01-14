using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.WebApi.Auth;
using MissAlise.WebApi.OneDrive;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("/")]
public class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> logger;
	private readonly AzureConfiguration config;
	private readonly GraphServiceClient graphServiceClient;	
	static AuthenticationResponse tokenResponse;
	public AuthController(ILogger<AuthController> logger, GraphServiceClient graphServiceClient, IOptions<AzureConfiguration> options)
	{
		this.logger = logger;
		this.graphServiceClient = graphServiceClient;
		config = options.Value;
	}

	[HttpGet]
	public async Task Get([FromServices] HttpClient client)
	{		
		var query = HttpContext.Request.QueryString;

		string authorizationCode = HttpContext.Request.Query["code"];
		if (int.TryParse(HttpContext.Request.Query["state"], out var userId))
		{ 
			 
		}

		var tokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

		var content = new FormUrlEncodedContent(new[]
		{
					new KeyValuePair<string, string>("client_id", config.ClientId),
					new KeyValuePair<string, string>("redirect_uri", config.RedirectUri+"signin-oidc"),
					new KeyValuePair<string, string>("client_secret", config.ClientSecret),
					new KeyValuePair<string, string>("code", authorizationCode),
					new KeyValuePair<string, string>("grant_type", "authorization_code")
				});
						
		var response = await client.PostAsync(tokenUrl, content);

		if (response.IsSuccessStatusCode)
		{
			var str = await response.Content.ReadAsStringAsync();
			tokenResponse = JsonSerializer.Deserialize<AuthenticationResponse>(str, _options);
			Console.WriteLine("Response: " + tokenResponse);
		}
		else
		{
			var res = await response.Content.ReadAsStringAsync();
			Console.WriteLine("Error: " + response.StatusCode);
		}
		//request.Headers.Add("Authorization", $"Bearer {tokenResponse?.AccessToken}");
	}
	static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
	{
		PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
		DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
	};	
}
