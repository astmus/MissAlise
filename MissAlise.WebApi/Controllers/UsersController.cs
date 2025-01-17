using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.Interfaces;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
	private readonly ILogger<UsersController> logger;
	private readonly GraphServiceClient graphServiceClient;
	public UsersController(ILogger<UsersController> logger, GraphServiceClient graphServiceClient)
	{
		this.logger = logger;
		this.graphServiceClient = graphServiceClient;		
	}

	[HttpGet("[action]")]
	public async Task<IActionResult> All(IUsersRepository users,CancellationToken cancel)
	{
		try
		{
			var me = await graphServiceClient.Me.GetAsync();			
			return Ok(me);
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
