using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MissAlise.Application.Dto;

namespace MissAlise.Services.Videos.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class VideosController : ControllerBase
	{
		private readonly ILogger<VideosController> _logger;
		private readonly IDistributedCache cache;
		private static int photoNum = int.MaxValue;

		public VideosController(ILogger<VideosController> logger, IDistributedCache cache)
		{
			_logger = logger;
			this.cache = cache;
		}

		[HttpGet]
		public async Task<IEnumerable<Video>> Get(CancellationToken cancel)
		{
			if (await cache.GetStringAsync("photo", cancel) is string cached)
				return JsonSerializer.Deserialize<Video[]>(cached) ?? Enumerable.Empty<Video>();

			IEnumerable<Video> result = [new Video() { Name = "Video " + photoNum-- }];
			var rawJson = JsonSerializer.Serialize(result);
			await cache.SetStringAsync("photo", rawJson, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
			});
			return result;
		}
	}
}
