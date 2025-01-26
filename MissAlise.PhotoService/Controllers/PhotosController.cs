using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MissAlise.Application.Dto;
using MissAlise.Application.Requests;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MissAlise.Services.Controllers;

namespace MissAlise.Services.Photos.Controllers
{
	public class PhotosController : ApiControllerBase
	{
		private readonly ILogger<PhotosController> logger;		
		private readonly IPhotoRepository db;

		public PhotosController(ILogger<PhotosController> logger, IPhotoRepository db)
		{
			this.logger = logger;			
			this.db = db;
		}

		[HttpGet]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<IEnumerable<PhotoSlimDto>>> Get([FromQuery] PagedQuery? pageQuery, CancellationToken cancel)
		{
			if (pageQuery is not PagedQuery query)
				query = new PagedQuery();

			var itemsPage = await db.GetPageAsync(query.Page, query.PerPage, cancel).ConfigureAwait(false);
			IEnumerable<PhotoSlimDto> result = itemsPage.Select(sel => new PhotoSlimDto() { Id = sel.Itemid.ToString(), Name = sel.Name ?? string.Empty }).ToArray();
			AddLinksHeaders(itemsPage);
			return Ok(result);
		}

		[HttpGet("{id:int}")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<IEnumerable<PhotoSlimDto>>> GetPhoto([FromRoute] int id, CancellationToken cancel)
		{			
			var photo = await db.FirstAsync(photo=>photo.Itemid == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound(id);

			photo.Folder = null;
			return Ok(photo);
		}

		[HttpGet("{id:int}/details")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<MediaInfo>> GetPhotoDetails([FromServices] IMediaService media, [FromRoute] int id, CancellationToken cancel)
		{
			var photo = await db.FirstAsync(photo => photo.Itemid == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound("photo "+id);

			var item = await db.GetItemById<Folder>(photo.Folderid, cancel);
			if (await db.GetItemById<Folder>(photo.Folderid, cancel) is not Folder folder)
				return NotFound("folder " + photo.Folderid);

			var path =  Path.Combine(folder.Path, folder.Name, photo.Name);

			var info = await media.GetMetaData(path, cancel).ConfigureAwait(false);

			return Ok(info);
		}
	}
}
