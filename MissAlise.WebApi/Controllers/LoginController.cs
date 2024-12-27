using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
	private readonly ILogger<LoginController> _logger;

	public LoginController(ILogger<LoginController> logger, GraphServiceClient graphServiceClient)
	{
		_logger = logger;
		_graphServiceClient = graphServiceClient;
	}
	private readonly GraphServiceClient _graphServiceClient;
	public async Task ConnectAsync()
	{
		await Task.CompletedTask;
	}
		
	[HttpGet("auth")]
	public async Task<IActionResult> GetMe()
	{
		try
		{
			var me = await _graphServiceClient.Me.GetAsync();
			return Ok(me);
		}
		catch (ServiceException ex)
		{
			return StatusCode(ex.ResponseStatusCode, ex.Message);
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
