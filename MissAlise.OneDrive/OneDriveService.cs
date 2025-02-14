using System.Web;
using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.OneDrive
{
	public class OneDriveService : IOneDriveService
	{
		private readonly AzureAd azureOptions;
		private readonly IOneDriveClient oneClient;
		private readonly IHandleContext ctx;

		public OneDriveService(IOptions<AzureAd> azureOptions, IOneDriveClient oneClient, IHandleContext ctx)
		{
			this.azureOptions = azureOptions.Value;
			this.oneClient = oneClient;
			this.ctx = ctx;
		}

		public Uri CreateAuthorizeLink(string stateIdentifier)
		{
			UriBuilder b = new UriBuilder(azureOptions.AuthPath);
			var query = HttpUtility.ParseQueryString(b.Query);
			query["scope"] = azureOptions.Scopes;
			query["client_id"] = azureOptions.ClientId;
			query["response_type"] = "code";
			query["redirect_uri"] = azureOptions.RedirectUri + azureOptions.CallbackPath;
			query["prompt"] = "select_account";
			query["state"] = stateIdentifier;
			b.Query = query.ToString();
			return b.Uri;
		}

		public async Task<User> GetOwnerInfo(CancellationToken cancel)
		{
			var items = await oneClient.RootItems(ctx, cancel);
			//var me = await client.Me.GetAsync(cancellationToken: cancel);
			//if (me != null)
			//	return new User() { Id = me.Id, DisplayName = me.DisplayName, GivenName = me.GivenName, Mail = me.Mail, PreferredLanguage = me.PreferredLanguage, Surname = me.Surname };

			return null;
		}

		public async Task<IEnumerable<Item>> GetRootItems(CancellationToken cancel)
		{
			//var resp = await client.Drives["ff"].Items[""].Delta.GetAsDeltaGetResponseAsync();
			//var items = await oneClient.RootItems(ctx, cancel);
			var res = await oneClient.RootDelta(ctx, cancel);
			List<Item> Items = new List<Item>();
			await foreach (var item in GetSynchronizator().WithCancellation(cancel))
			{
				Items.Add(item);
			}
			//if (res.Content != null)

			return default;
		}

		public DataSynchronizator GetSynchronizator()
		{
			return new OneDriveDataSynchronizator(oneClient, ctx);
		}
	}

	public class OneDriveDataSynchronizator : DataSynchronizator
	{
		private DriveItemDeltaGetResponse driveDelta;
		private readonly IOneDriveClient client;
		private readonly IHandleContext ctx;

		public OneDriveDataSynchronizator(IOneDriveClient client, IHandleContext ctx)
		{
			this.client = client;
			this.ctx = ctx;
		}

		public override async IAsyncEnumerator<Item> GetAsyncEnumerator(CancellationToken cancellationToken = default)
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

		static Item CreateItem(DriveItem item)
		{
			Item result = item switch
			{
				{ Folder: not null } => new Entities.OneDrive.Folder()
				{
					Name = item.Name,
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
				{ File: not null } => new Entities.OneDrive.File()				
			};

			result.Name = item.Name;
			result.MimeType = item.File?.MimeType;
			result.CreatedDateTime = item.FileSystemInfo.CreatedDateTime;
			result.ModifieDateTime = item.FileSystemInfo.LastModifiedDateTime;

			if (result is not Entities.OneDrive.File file)
				return result;

			file.Size = item.Size;			
			file.Caption = Path.GetFileNameWithoutExtension(item.Name);
			file.Extension = Path.GetExtension(item.Name);

			return file;
		}
	}
}
