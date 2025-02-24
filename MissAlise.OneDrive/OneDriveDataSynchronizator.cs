using System.Web;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;

namespace MissAlise.OneDrive
{
	public class OneDriveDataSynchronizator : DataSynchronizator
	{
		private DriveItemsDelta driveDelta;
		private readonly IOneDriveClient client;
		private readonly IHandleContext ctx;

		public OneDriveDataSynchronizator(IOneDriveClient client, IHandleContext ctx)
		{
			this.client = client;
			this.ctx = ctx;
		}

		public override async IAsyncEnumerator<ItemInfo> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		{
			var deltaResponse = await client.RootDelta(ctx, cancellationToken).ConfigureAwait(false);
			driveDelta = deltaResponse.Content;

			while (driveDelta != null && driveDelta.Value.Any() && !cancellationToken.IsCancellationRequested)
			{
				foreach (var item in driveDelta.Value)
					yield return CreateItem(item);

				if (Uri.TryCreate(driveDelta.OdataNextLink, default, out var nextUrl) == false)
					yield break;

				string token = HttpUtility.ParseQueryString(nextUrl.Query).Get("token");

				var response = await client.DeltaShift(token, ctx, cancellationToken).ConfigureAwait(false);
				driveDelta = response.Content;
			}
		}

		static ItemInfo CreateItem(DriveItem item)
		{
			ItemInfo result = item switch
			{
				{ Folder: not null } => new Entities.OneDrive.Folder()
				{					
					Title = $"[{item.Name}]",
					Path = item.ParentReference.Path,
					MimeType = "folder"
				},
				{ Image: not null } => new Entities.OneDrive.Photo()
				{
					Width = item.Image.Width,
					Height = item.Image.Height,
					Orientation = item.Photo.Orientation,
					Fnumber = item.Photo.FNumber,
					Iso = item.Photo.Iso,
					Cameramake = item.Photo.CameraMake,
					Cameramodel = item.Photo.CameraModel,
					Takendatetime = item.Photo.TakenDateTime?.DateTime
				},
				{ Video: not null } => new Entities.OneDrive.Video()
				{
					Width = item.Video.Width,
					Height = item.Video.Height,
					Duration = TimeSpan.FromMilliseconds(item.Video.Duration ?? 0),
					Audiochannels = item.Video.AudioChannels,
					Audiosamplespersecond = item.Video.AudioSamplesPerSecond,
					Bitrate = item.Video.Bitrate,
					Fourcc = item.Video.FourCC,
					Framerate = item.Video.FrameRate
				},
				{ Audio: not null } => new Entities.OneDrive.Audio()
				{
					Duration = item.Audio.Duration,
					Album = item.Audio.Album,
					Artist = item.Audio.Artist,
					Track = item.Audio.Track,
					TrackTitle = item.Audio.Title,
					TrackCount = item.Audio.TrackCount,
					Year = item.Audio.Year,
					Genre = item.Audio.Genre
				},
				{ File: not null } => new Entities.OneDrive.DataFile()
			};

			result.Title = item.Name;
			result.MimeType = item.File?.MimeType;
			result.CreatedDateTime = item.FileSystemInfo.CreatedDateTime;
			result.ModifieDateTime = item.FileSystemInfo.LastModifiedDateTime;

			if (result is not Entities.OneDrive.DataFile file)
				return result;

			file.Size = item.Size;
			file.Name = Path.GetFileNameWithoutExtension(item.Name);
			file.Extension = Path.GetExtension(item.Name);

			return file;
		}
	}
}
