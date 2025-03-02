using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MissAlise.Application.Common;
using MissAlise.Application.Dto;
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
		public async Task<ActionResult<IEnumerable<PhotoSlim>>> Get([FromQuery] PagedQuery? pageQuery, CancellationToken cancel)
		{
			if (pageQuery is not PagedQuery query)
				query = new PagedQuery();

			var itemsPage = await db.GetPageAsync(query.Page, query.PerPage, cancel).ConfigureAwait(false);
			IEnumerable<PhotoSlim> result = itemsPage.Select(sel => new PhotoSlim() { Id = sel.Id.ToString(), Name = sel.Name ?? string.Empty }).ToArray();
			AddLinksHeaders(itemsPage);
			return Ok(result);
		}

		[HttpGet("{id:string}")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<IEnumerable<Application.Dto.Photo>>> GetPhoto([FromRoute] string id, CancellationToken cancel)
		{
			var photo = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound(id);

			photo.Parent = null;
			return Ok(photo);
		}

		[HttpGet("{id:string}/details")]
		[OutputCache(Duration = 60)]
		public async Task<ActionResult<MediaInfo>> GetPhotoDetails([FromServices] IMediaService media, [FromRoute] string id, CancellationToken cancel)
		{
			var photo = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (photo == null)
				return NotFound("photo " + id);

			if (await db.GetById<Folder>(photo.Id, cancel) is not Folder folder)
				return NotFound("folder " + photo.Id);

			var path = Path.Combine(folder.Path, folder.Title ?? string.Empty, photo.Name);

			var info = await media.GetMetaDataAsync(path, cancel).ConfigureAwait(false);

			return Ok(info);
		}

		[HttpPost("{id:string}")]
		public async Task<ActionResult<MediaInfo>> CreateImage([FromServices] IMediaService media, [FromRoute] string id, [FromBody] ApplyImageParams args, CancellationToken cancel)
		{
			var img = await db.FirstAsync(photo => photo.Id == id, cancel).ConfigureAwait(false);
			if (img == null)
				return NotFound("image " + id);

			if (await db.GetById<Folder>(img.Id, cancel) is not Folder folder)
				return NotFound("folder " + img.Id);

			args.Format ??= Path.GetExtension(img.Name) switch
			{
				".jpeg" or ".jpg" => ImageFormat.jpeg,
				".png" => ImageFormat.png,
				".bmp" => ImageFormat.bmp,
				".tiff" => ImageFormat.tiff,
				".webp" => ImageFormat.webp,
				_ => throw new ArgumentException("File format exception " + img.Name)
			};

			var targetPath = Path.Combine(folder.Path, folder.Title ?? string.Empty, $"{args.Name}.{args.Format}");
			if (System.IO.File.Exists(targetPath))
				return Conflict($"File name conflict '{Path.GetFileName(targetPath)}'");

			var path = Path.Combine(folder.Path, folder.Title ?? string.Empty, img.Name);

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
				Name = info.Format.Filename,
				Size = int.Parse(info.Format.Size),
				Parent = folder,
				MimeType = "image/" + info.Streams[0].CodecName,
				CreatedDateTime = DateTimeOffset.UtcNow,
				ModifieDateTime = DateTimeOffset.UtcNow
			};

			folder.Children.Add(photo);
			return Ok(info);
		}
	}
}
