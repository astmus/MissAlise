using System.Runtime.CompilerServices;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace MissAlise.Worker.Features.Sync
{
	public class SyncCommandHandler : AsyncHandlerBase<SyncCommand>
	{		
		private readonly GraphServiceClient client;	
		private readonly ILogger<SyncCommandHandler> log;
		const string ROOT_SYNC_PATH = @"M:\Sync\";
		public SyncCommandHandler(GraphServiceClient client, ILogger<SyncCommandHandler> log)
		{			
			this.client = client;			
			this.log = log;
		}
		static readonly string[] fields = ["name", "folder", "parentReference", "size", "id", "createdDateTime", "file", "@microsoft.graph.downloadUrl", "fileSystemInfo", "photo", "image", "audio"];
		protected override async Task HandleAsync(SyncCommand data, CancellationToken cancel)
		{
			try
			{
				var drive = await client.Me.Drive.GetAsync().ConfigureAwait(false);
				var driveRoot = await client.Drives[drive.Id].Root.GetAsync().ConfigureAwait(false);
				string userRoot = Path.Combine(ROOT_SYNC_PATH, drive.Owner.User.DisplayName);
				var children = await client.Drives[drive.Id].Items[driveRoot.Id].Children.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);

				foreach (var childItem in children.Value)
				{
					string folderPath = string.Empty;
					await foreach (var item in ListFolderContentsWithPagination(client, cancel, childItem))
					{
						if (item.Folder != null)
							folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""), item.Name);
						else
							folderPath = Path.Combine(userRoot, Path.Combine(item.ParentReference.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray()).Replace(":", ""));

						if (!Directory.Exists(folderPath))
							Directory.CreateDirectory(folderPath);

						if (item.File != null)
							await File.WriteAllTextAsync(Path.Combine(folderPath, item.Name), "filePath").ConfigureAwait(false);
					}
				}
			}
			catch (Exception error)
			{
				log.LogError(error, "Sync user folder error {error}", error.Message);
			}
		}

		public async IAsyncEnumerable<DriveItem> ListFolderContentsWithPagination(GraphServiceClient graphClient, [EnumeratorCancellation] CancellationToken cancel, DriveItem currentItem = null)
		{
			if (currentItem == null)
				yield break;

			var childrenRequest = graphClient.Drives[currentItem.ParentReference.DriveId].Items[currentItem.Id].Children;
			var children = await childrenRequest.GetAsync(query => query.QueryParameters.Select = fields).ConfigureAwait(false);

			if ((children.Value?.Count ?? 0) == 0)
				yield return currentItem;
			else
				foreach (var item in children.Value)
				{
					yield return item;
					await foreach (var subItem in ListFolderContentsWithPagination(graphClient, cancel, item))
						yield return subItem;
				}

			//await PageIterator<DriveItem, DriveItemCollectionResponse>.CreatePageIterator(graphClient, children, item => {			
			//Console.WriteLine($"Name: {item.Name}, Type: {(item.Folder != null ? "Folder" : "File")}");
			//return true; //}).IterateAsync(cancel);
		}
	}
}
