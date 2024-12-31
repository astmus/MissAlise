using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using MissAlise.WebApi.Auth;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]

public class LoginController : ControllerBase
{
	private readonly ILogger<LoginController> _logger;
	private readonly AzureConfiguration config;
	private readonly GraphServiceClient _graphServiceClient;
	public LoginController(ILogger<LoginController> logger, GraphServiceClient graphServiceClient, IOptions<AzureConfiguration> options)
	{
		_logger = logger;
		_graphServiceClient = graphServiceClient;	
		config = options.Value;
	}

	public async Task ConnectAsync()
	{
		await Task.CompletedTask;
	}
		
	[HttpGet("auth")]	
	public async Task<IActionResult> GetMe(CancellationToken cancel)
	{
		try
		{			
			//var drive = users.Value.First().Drive;
			var a = await _graphServiceClient.Me.GetAsync();
			var data = await _graphServiceClient.Drives.GetAsync();
			//var me = await _graphServiceClient.Drives.GetAsync();
			return Ok(data);
		}
		catch (ServiceException ex)
		{
			return StatusCode(ex.ResponseStatusCode, ex.Message);
		}
		catch (Exception error)
		{
			int i = 0;
			return StatusCode(error.HResult, error.Message);
		}
	}

	//[HttpPost("upload")]
	//public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
	//{
	//	try
	//	{
	//		using var stream = file.OpenReadStream();
	//		var uploadedFile = await _graphServiceClient.Me.Drive.Root
	//			.ItemWithPath(file.FileName)
	//			.Content
	//			.Request()
	//			.PutAsync<DriveItem>(stream);

	//		return Ok(uploadedFile);
	//	}
	//	catch (ServiceException ex)
	//	{
	//		return StatusCode((int)ex.StatusCode, ex.Message);
	//	}
	//}
}
