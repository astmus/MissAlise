using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("/")]
public class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> logger;	
	private readonly GraphServiceClient graphServiceClient;	
	
	public AuthController(ILogger<AuthController> logger, GraphServiceClient graphServiceClient)
	{
		this.logger = logger;
		this.graphServiceClient = graphServiceClient;		
	}

	[HttpGet]
	public void Get()
	{		
		
	}
	
}
