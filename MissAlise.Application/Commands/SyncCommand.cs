using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.Interfaces;
using MissAlise.ValueObjects;

namespace MissAlise.Application.Commands
{
	public record SyncCommand(UserId user) : ICommand;

	public class SyncCommandHandler : AsyncHandlerBase<SyncCommand>, ICommandHandler<SyncCommand>
	{
		private readonly IHttpClientFactory factory;

		// не забыть перенести в OneDrive библиотеку, клиент и прочее что касается OneDrive
		//private readonly GraphServiceClient client;
		private readonly ILogger<SyncCommandHandler> log;
		private readonly IUserRepository usersRepository;
		private readonly IOneDriveService oneDrive;
		private readonly IHandleContext ctx;
		const string ROOT_SYNC_PATH = @"D:\Sync\"; // и вот это барахло тоже убрать
		public SyncCommandHandler(IHttpClientFactory factory, ILogger<SyncCommandHandler> log, IUserRepository usersRepository, IOneDriveService oneDrive, IHandleContext ctx)
		{
			this.factory = factory;
			//this.client = client;
			this.log = log;
			this.usersRepository = usersRepository;
			this.oneDrive = oneDrive;
			this.ctx = ctx;
		}
		static readonly string[] fields = ["name", "folder", "parentReference", "size", "id", "createdDateTime", "file", "@microsoft.graph.downloadUrl", "fileSystemInfo", "photo", "image", "audio", "video"];
		protected override async Task RunHandleAsync(SyncCommand data, CancellationToken cancel)
		{
			try
			{
				await Task.Delay(50);

				//	var items = await oneDrive.GetRootItems(cancel);				
				//var sync = oneDrive.GetSynchronizator().ToBlockingEnumerable().Where(w=> w is Photo).ToList();
				var i = 0;
				using var http = factory.CreateClient("onecredentials");
				//var user = await client.Me.GetAsync();
				//var drive = await client.Me.Drive.GetAsync().ConfigureAwait(false);
				////var driveItems = await client.Me.Drive.GetAsync().ConfigureAwait(false);
				//var stroage = await usersRepository.GetUserStorageAsync(new Entities.OneDrive.User()
				//{
				//	Id = user.Id,
				//	DisplayName = user.DisplayName,
				//	GivenName = user.GivenName,
				//	Mail = user.Mail,
				//	PreferredLanguage = user.PreferredLanguage,
				//	Surname = user.Surname
				//}, cancel);
				//var driveRoot = await client.Drives[drive.Id].Root.GetAsync().ConfigureAwait(false);

				//var userRoot = Path.Combine(ROOT_SYNC_PATH, drive.Owner.User.DisplayName);
				//stroage.RootFolder ??= new Entities.OneDrive.Folder()
				//{
				//	Title = driveRoot.Id,
				//	Path = userRoot
				//};
				////stroage.RootFolder.Folders.Clear();
				////stroage.RootFolder.Files.Clear();
				////var children = await client.Drives[drive.Id].Items[driveRoot.Id].Children.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);
				////var sorted = children.Value.OrderBy(item => item.Folder == null);

				//var folderPath = string.Empty;
				//List<Task> tasks = new List<Task>();
				//await foreach (var item in ListFolderContentsWithPagination(client, cancel, stroage.RootFolder, driveRoot))
				//{
				//	if (item.Folder != null)
				//		folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""), item.Name);
				//	else
				//		folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""));

				//	if (!Directory.Exists(folderPath))
				//		Directory.CreateDirectory(folderPath);

				//	var task = DownloadIfFile(item, Path.Combine(folderPath, item.Name), cancel);
				//	tasks.Add(task);
				//	//if (item.File != null && System.IO.File.Exists(Path.Combine(folderPath, item.Name)) == false)
				//	//	await using (var streamWriter = System.IO.File.Create(Path.Combine(folderPath, item.Name), 4096))
				//	//	{
				//	//		var stream = await client.Drives[item.ParentReference.DriveId].Items[item.Id].Content.GetAsync();
				//	//		await stream.CopyToAsync(streamWriter, 4096, cancel);
				//	//		//await item.DeleteAsync(cancellationToken: cancel);
				//	//	}
				//	//await System.IO.File.WriteAllTextAsync(Path.Combine(folderPath, item.Name), "filePath").ConfigureAwait(false);
				//}
				//int i = 0;
				//await Task.WhenAll(tasks).ConfigureAwait(false);0
				//await stroage.SaveAsync();
			}
			catch (Exception error)
			{
				log.LogError(error, "Sync user folder error {error}", error.Message);
			}
		}

		public async Task<Result> Handle(SyncCommand request, CancellationToken cancel)
		{
			var principal = ctx.Get<MissAlise.Entities.Identity.SyncPrincipal>();
			if (principal is null)
				return Result.Fail("Sync principal not set");
			var repository = await usersRepository.GetStorageAsync(principal.Profile.UserId, cancel);

			await using var pipeline = new DriveSyncPipeline(
				oneDrive,
				ROOT_SYNC_PATH,
				ROOT_SYNC_PATH,
				cancel,
				parallelism: 4);

			await oneDrive.HandleDeltaDriveItemsAsync(
				item => pipeline.PostAsync(item, cancel),
				cancel);

			// 2️⃣ Закрываем вход (важно!)
			await pipeline.CompleteAsync();

			// 3️⃣ Дожидаемся полной обработки
			await pipeline.Completion;

			return Result.Successful;
		}

		//public async Task DownloadIfFile(DriveItem item, string path, CancellationToken cancel)
		//{
		//	if (item.File != null && System.IO.File.Exists(path) == false)
		//		await using (var streamWriter = System.IO.File.Create(path, 4096))
		//		{
		//			var stream = await client.Drives[item.ParentReference.DriveId].Items[item.Id].Content.GetAsync();
		//			await stream.CopyToAsync(streamWriter, 4096, cancel);
		//			//await item.DeleteAsync(cancellationToken: cancel);
		//		}
		//}

		//public async IAsyncEnumerable<DriveItem> ListFolderContentsWithPagination(GraphServiceClient graphClient, [EnumeratorCancellation] CancellationToken cancel, Entities.OneDrive.Folder folder, DriveItem currentItem = null)
		//{
		//	if (currentItem == null)
		//		yield break;

		//var childrenRequest = graphClient.Drives[currentItem.ParentReference.DriveId].Items[currentItem.Id].Children;

		//	var children = await childrenRequest.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);

		//	if ((children.Value?.Count ?? 0) == 0)
		//		yield return currentItem;
		//	else
		//		foreach (var item in children.Value.OrderBy(ob => ob.Folder == null))
		//		{
		//			yield return item;

		//			if (item.Folder != null)
		//				await foreach (var subItem in ListFolderContentsWithPagination(graphClient, cancel, AddItem(folder, item), item))
		//					yield return subItem;
		//			else
		//				AddItem(folder, item);
		//		}
		//	//await PageIterator<DriveItem, DriveItemCollectionResponse>.CreatePageIterator(graphClient, children, item => {			
		//	//Console.WriteLine($"Name: {item.Name}, Type: {(item.Folder != null ? "Folder" : "File")}");
		//	//return true; //}).IterateAsync(cancel);
		//}		

		//Entities.OneDrive.Folder AddItem(Entities.OneDrive.Folder folder, DriveItem item)
		//{
		//	if (item.Folder != null)
		//	{
		//		var newFolder = new Entities.OneDrive.Folder()
		//		{
		//			Name = item.Name,
		//			Title = $"[{item.Name}]",
		//			Path = item.ParentReference.Path,
		//			MimeType = "folder",
		//			Parent = folder
		//		};
		//		folder.Folders.Add(newFolder);
		//		return newFolder;
		//	}
		//	else
		//	{
		//		var file = item switch
		//		{
		//			{ Image: { } } => new Entities.OneDrive.Photo()
		//			{
		//				Width = item.Image.Width,
		//				Height = item.Image.Height,
		//				Orientation = item.Photo.Orientation,
		//				Fnumber = item.Photo.FNumber,
		//				Iso = item.Photo.Iso,
		//				Cameramake = item.Photo.CameraMake,
		//				Cameramodel = item.Photo.CameraModel
		//			},
		//			{ Video: { } } => new Entities.OneDrive.Video()
		//			{
		//				Width = item.Video.Width,
		//				Height = item.Video.Height,
		//				Duration = TimeSpan.FromMilliseconds(item.Video.Duration ?? 0),
		//				Audiochannels = item.Video.AudioChannels,
		//				Audiosamplespersecond = item.Video.AudioSamplesPerSecond,
		//				Bitrate = item.Video.Bitrate,
		//				Fourcc = item.Video.FourCC,
		//				Framerate = item.Video.FrameRate
		//			},
		//			{ Audio: { } } => new Entities.OneDrive.Audio()
		//			{
		//				Duration = item.Audio.Duration,
		//				Album = item.Audio.Album,
		//				Artist = item.Audio.Artist,
		//				Track = item.Audio.Track,
		//				TrackTitle = item.Audio.Title,
		//				TrackCount = item.Audio.TrackCount,
		//				Year = item.Audio.Year,
		//				Genre = item.Audio.Genre
		//			},
		//			_ => new Entities.OneDrive.File()
		//		};

		//		file.Name = item.Name;
		//		file.Size = item.Size;
		//		file.Folder = folder;
		//		file.Caption = Path.GetFileNameWithoutExtension(item.Name);
		//		file.MimeType = item.File.MimeType;
		//		file.Extension = Path.GetExtension(item.Name);
		//		file.CreatedDateTime = item.FileSystemInfo.CreatedDateTime;
		//		file.ModifieDateTime = item.FileSystemInfo.LastModifiedDateTime;
		//		folder.Files.Add(file);
		//		return folder;
		//	}
		//}
	}
}
