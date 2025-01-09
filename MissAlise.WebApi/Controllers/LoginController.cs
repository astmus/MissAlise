using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.WebApi.Auth;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
	private readonly ILogger<LoginController> logger;
	private readonly AzureConfiguration config;
	private readonly GraphServiceClient graphServiceClient;
	public LoginController(ILogger<LoginController> logger, GraphServiceClient graphServiceClient, IOptions<AzureConfiguration> options)
	{
		this.logger = logger;
		this.graphServiceClient = graphServiceClient;
		config = options.Value;
	}

	[HttpGet("auth")]
	public async Task<IActionResult> GetMe(CancellationToken cancel)
	{
		try
		{
			var me = await graphServiceClient.Me.GetAsync();
			var a = await graphServiceClient.Me.Drives.GetAsync();
			var res = JsonSerializer.Serialize(me, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
			var data = await graphServiceClient.Drives[a.Value[0].Id].Root.GetAsync();
			var childrewn = await graphServiceClient.Drives[a.Value[0].Id].Items[data.Id].Children.GetAsync();
			var sizes = childrewn.Value.OrderByDescending(item => item.Size).ToList();
			var ittt = await graphServiceClient.Drives[a.Value[0].Id].Items["65FA3479348E5262%21226388"].Children.GetAsync();
			var list = ittt.Value.Select(sel => graphServiceClient.Drives[a.Value[0].Id].Items[sel.Id]).ToImmutableArray();
			Stopwatch sw = Stopwatch.StartNew();
			await Parallel.ForEachAsync(list,
			//list.AsParallel().WithMergeOptions(ParallelMergeOptions.NotBuffered).WithCancellation(cancel).ForAll(
			async (item, cancel) =>
			{
				var info = await item.GetAsync();
				if (info.Photo is Photo photo && photo.TakenDateTime is DateTimeOffset timeOffset)
				{
					var name = timeOffset.ToString("yyyy.MM");
					var path = Path.Combine("D:\\Images", name);
					if (Directory.Exists(Path.Combine("D:\\Images", name)) == false)
						Directory.CreateDirectory(path);
					path = Path.Combine(path, info.Name);
					await using (var streamWriter = System.IO.File.Create(path, 4096))
					{
						var stream = await item.Content.GetAsync(cancellationToken: cancel);
						await stream.CopyToAsync(streamWriter, 4096, cancel);
						//await item.DeleteAsync(cancellationToken: cancel);
					}
				}
			});
			sw.Stop();

			//foreach (var item in list)
			//{
			//	var info = await item.GetAsync();
			//	if (info.Photo is Photo photo && photo.TakenDateTime is DateTimeOffset timeOffset)
			//	{
			//		var name = timeOffset.ToString("yyyy.MM");
			//		var path = Path.Combine("D:\\Images", name);
			//		if (Directory.Exists(Path.Combine("D:\\Images", name)) == false)
			//			Directory.CreateDirectory(path);
			//		path = Path.Combine(path, info.Name);
			//		await using (var streamWriter = System.IO.File.Create(path))
			//		{
			//			var stream = await item.Content.GetAsync(cancellationToken: cancel);						
			//			await stream.CopyToAsync(streamWriter, cancel);						
			//		}
			//		sb.AppendLine(info.Name);
			//		//Console.WriteLine(JsonSerializer.Serialize(photo));
			//	}
			//}
			//var me = await _graphServiceClient.Drives.GetAsync();
			return Ok(sw.Elapsed);
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
