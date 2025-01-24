using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.Application.UseCases.Sync
{
	public class SyncCommandHandler : AsyncHandlerBase<SyncCommand>
	{
		// не забыть перенести в OneDrive библиотеку, клиент и прочее что касается OneDrive
		private readonly GraphServiceClient client;
		private readonly ILogger<SyncCommandHandler> log;
		private readonly IUsersRepository usersRepository;
		const string ROOT_SYNC_PATH = @"M:\Sync\"; // и вот это барахло тоже убрать
		public SyncCommandHandler(GraphServiceClient client, ILogger<SyncCommandHandler> log, IUsersRepository usersRepository)
		{
			this.client = client;
			this.log = log;
			this.usersRepository = usersRepository;
		}
		static readonly string[] fields = ["name", "folder", "parentReference", "size", "id", "createdDateTime", "file", "@microsoft.graph.downloadUrl", "fileSystemInfo", "photo", "image", "audio", "video"];
		protected override async Task HandleAsync(SyncCommand data, CancellationToken cancel)
		{
			try
			{
				var user = await client.Me.GetAsync();
				var drive = await client.Me.Drive.GetAsync().ConfigureAwait(false);
				var stroage = await usersRepository.GetUserStorageAsync(new Entities.OneDrive.User()
				{
					Id = user.Id,
					DisplayName = user.DisplayName,
					GivenName = user.GivenName,
					Mail = user.Mail,
					PreferredLanguage = user.PreferredLanguage,
					Surname = user.Surname
				}, cancel);
				
				var driveRoot = await client.Drives[drive.Id].Root.GetAsync().ConfigureAwait(false);
				var userRoot = Path.Combine(ROOT_SYNC_PATH, drive.Owner.User.DisplayName);
				stroage.RootFolder ??= new Entities.OneDrive.Folder()
				{
					Title = driveRoot.Id,
					Path = userRoot
				};
				//var children = await client.Drives[drive.Id].Items[driveRoot.Id].Children.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);
				//var sorted = children.Value.OrderBy(item => item.Folder == null);

				var folderPath = string.Empty;
				await foreach (var item in ListFolderContentsWithPagination(client, cancel, stroage.RootFolder, driveRoot))
				{
					if (item.Folder != null)
						folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""), item.Name);
					else
						folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""));

					if (!Directory.Exists(folderPath))
						Directory.CreateDirectory(folderPath);

					if (item.File != null)
						await using (var streamWriter = System.IO.File.Create(Path.Combine(folderPath, item.Name), 4096))
						{
							var stream = await client.Drives[item.ParentReference.DriveId].Items[item.Id].Content.GetAsync();
							await stream.CopyToAsync(streamWriter, 4096, cancel);
							//await item.DeleteAsync(cancellationToken: cancel);
						}
					//await System.IO.File.WriteAllTextAsync(Path.Combine(folderPath, item.Name), "filePath").ConfigureAwait(false);
				}
				int i = 0;
				await stroage.SaveAsync();
			}
			catch (Exception error)
			{
				log.LogError(error, "Sync user folder error {error}", error.Message);
			}
		}

		public async IAsyncEnumerable<DriveItem> ListFolderContentsWithPagination(GraphServiceClient graphClient, [EnumeratorCancellation] CancellationToken cancel, Entities.OneDrive.Folder folder, DriveItem currentItem = null)
		{
			if (currentItem == null)
				yield break;

			var childrenRequest = graphClient.Drives[currentItem.ParentReference.DriveId].Items[currentItem.Id].Children;	
			
			var children = await childrenRequest.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);

			if ((children.Value?.Count ?? 0) == 0)
				yield return currentItem;
			else
				foreach (var item in children.Value.OrderBy(ob => ob.Folder == null))
				{
					yield return item;

					if (item.Folder != null)
						await foreach (var subItem in ListFolderContentsWithPagination(graphClient, cancel, AddItem(folder, item), item))
							yield return subItem;
					else
						AddItem(folder, item);						
				}

			//await PageIterator<DriveItem, DriveItemCollectionResponse>.CreatePageIterator(graphClient, children, item => {			
			//Console.WriteLine($"Name: {item.Name}, Type: {(item.Folder != null ? "Folder" : "File")}");
			//return true; //}).IterateAsync(cancel);
		}

		Entities.OneDrive.Folder AddItem(Entities.OneDrive.Folder folder, DriveItem item)
		{
			if (item.Folder != null)
			{
				var newFolder = new Entities.OneDrive.Folder()
				{
					Name = item.Name,
					Title = item.Id,
					Path = item.ParentReference.Path,
					Parent = folder
				};
				folder.Folders.Add(newFolder);
				return newFolder;
			}
			else
			{
				var file = item switch
				{
					{ Image: { } } => new Entities.OneDrive.Photo()
					{
						Width = item.Image.Width,
						Height = item.Image.Height,
						Orientation = item.Photo.Orientation,
						Fnumber = item.Photo.FNumber,
						Iso = item.Photo.Iso,
						Cameramake = item.Photo.CameraMake,
						Cameramodel = item.Photo.CameraModel
					},
					{ Video: { } } => new Entities.OneDrive.Video()
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
					{ Audio: { } } => new Entities.OneDrive.Audio()
					{
						Duration = item.Audio.Duration,
						Album = item.Audio.Album,
						Artist = item.Audio.Artist,
						Track = item.Audio.Track,
						TrackTitle = item.Audio.Title
					},
					_ => new Entities.OneDrive.File()
				};

				file.Name = item.Name;
				file.Size = Convert.ToInt32(item.Size);
				file.Folder = folder;
				file.Title = item.Id;
				file.Mimetype = item.File.MimeType;
				file.Createddatetime = item.FileSystemInfo.CreatedDateTime;
				file.Modifiedatetime = item.FileSystemInfo.LastModifiedDateTime;
				folder.Files.Add(file);
				return folder;
			}
		}
	}
}
