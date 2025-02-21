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
			IEnumerable<PhotoSlimDto> result = itemsPage.Select(sel => new PhotoSlimDto() { Id = sel.Id.ToString(), Name = sel.Caption ?? string.Empty }).ToArray();
			AddLinksHeaders(itemsPage);
			return Ok(result);
		}

		[HttpGet("{id:int}")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<IEnumerable<PhotoDto>>> GetPhoto([FromRoute] int id, CancellationToken cancel)
		{
			var photo = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound(id);

			photo.Folder = null;
			return Ok(photo);
		}

		[HttpGet("{id:int}/details")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<MediaInfo>> GetPhotoDetails([FromServices] IMediaService media, [FromRoute] int id, CancellationToken cancel)
		{
			var photo = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound("photo " + id);

			if (await db.GetById<Folder>(photo.FolderId, cancel) is not Folder folder)
				return NotFound("folder " + photo.FolderId);

			var path = Path.Combine(folder.Path, folder.Title, photo.Caption);

			var info = await media.GetMetaDataAsync(path, cancel).ConfigureAwait(false);

			return Ok(info);
		}

		[HttpPost("{id:int}")]
		public async Task<ActionResult<MediaInfo>> CreateImage([FromServices] IMediaService media, [FromRoute] int id, [FromBody] ApplyImageParams args, CancellationToken cancel)
		{
			var img = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (img == null)
				return NotFound("image " + id);

			if (await db.GetById<Folder>(img.FolderId, cancel) is not Folder folder)
				return NotFound("folder " + img.FolderId);

			args.Format ??= Path.GetExtension(img.Caption) switch
			{
				".jpeg" or ".jpg" => ImageFormat.jpeg,
				".png" => ImageFormat.png,
				".bmp" => ImageFormat.bmp,
				".tiff" => ImageFormat.tiff,
				".webp" => ImageFormat.webp,
				_ => throw new ArgumentException("File format exception "+img.Caption)
			};
			
			var targetPath = Path.Combine(folder.Path, folder.Title, $"{args.Name}.{args.Format}");
			if (System.IO.File.Exists(targetPath))
				return Conflict($"File name conflict '{Path.GetFileName(targetPath)}'");

			var path = Path.Combine(folder.Path, folder.Title, img.Caption);

			var info = await media.ImageFromAsync(path, args, cancel).ConfigureAwait(false);
			if (info.Anomaly != null)
				return Problem(info.Anomaly);

			var photo = new Entities.OneDrive.Photo()
			{
				Width = info.Streams[0].Width,
				Height = info.Streams[0].Height,
				Orientation = img.Orientation,
				Fnumber = img.Fnumber,
				Iso = img.Iso,
				Cameramake = img.Cameramake,
				Cameramodel = img.Cameramodel,
				Caption = info.Format.Filename,
				Size = int.Parse(info.Format.Size),
				Folder = folder,
				MimeType = "image/" + info.Streams[0].CodecName,
				CreatedDateTime = DateTimeOffset.UtcNow,
				ModifieDateTime = DateTimeOffset.UtcNow
			};

			folder.Files.Add(photo);
			return Ok(info);
		}
	}
}
