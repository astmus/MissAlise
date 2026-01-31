using System.Web;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

//using Microsoft.Graph;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Identity;
using MissAlise.Interfaces;
using MissAlise.ValueObjects;
using MissAlise.Entities.Media;
using MissAlise.OneDrive.Auth;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Mapping;
using MissAlise.OneDrive.Models;
using MissAlise.ValueObjects.Media;

namespace MissAlise.OneDrive
{
	internal class OneDriveService : IOneDriveService
	{
		private readonly AzureAd _azure;
		private readonly IHandleContext _ctx;
		private readonly ApiClient _client;
		private readonly IOneDriveTokenService _tokenService;
		private readonly IAccessCredentialsStore _accessStore;

		public OneDriveService(IOptions<AzureAd> azure, ApiClient client, IHandleContext ctx, IOneDriveTokenService tokenService, IAccessCredentialsStore accessStore)
		{
			_azure = azure.Value;
			_ctx = ctx;
			_client = client;
			_tokenService = tokenService;
			_accessStore = accessStore;
		}

		public async Task<User> GetOwnerInfo(CancellationToken cancel)
		{
			var me = await _client.Me.GetAsync(cancellationToken: cancel);			

			if (me != null)
				return new User() { Id = me.Id, DisplayName = me.DisplayName, GivenName = me.GivenName, Mail = me.Mail, PreferredLanguage = me.PreferredLanguage, Surname = me.Surname };

			return null;
		}

		public Task<Stream?> GetItemContent(string parentId, string itemId, CancellationToken cancel)
		{ 
			return _client.Drives[parentId].Items[itemId].Content.GetAsync(cancellationToken: cancel);
		}

		public async Task<string> HandleDeltaDriveItemsAsync(Action<MediaItem> handleDelegate, CancellationToken cancel)
		{
			//var childrenRequest = _client.Drives["Me"].Items["Root"].Delta;
			var childrenRequest = _client.Drives["Me"].Items["Root"].Delta;
			var items = await childrenRequest.GetAsDeltaGetResponseAsync<DriveDeltaItems>(r => r.QueryParameters.Select = new[]
				{
					"id",
					"name",
					"description",
					"createdDateTime",
					"lastModifiedDateTime",
					"createdBy",
					"lastModifiedBy",
					"parentReference",
					"file",
					"image",
					"video",
					"photo",
					"folder",
					"size",
					"webUrl",
					"deleted",
					"fileSystemInfo"
				});

			PageIterator<DriveItem, DriveDeltaItems> iterator = null;

			int varo = 0;
			
			iterator = PageIterator<DriveItem, DriveDeltaItems>.CreatePageIterator(_client.Adapter, items, callback: 
				item =>
					{
						varo++;
						if (item.Folder != null)
							return true;
				
						var itemInfo = CreateItem(item);
						if (itemInfo != null)
							handleDelegate(itemInfo);
						return true;
					}
			);

			await iterator.IterateAsync(cancel);
			
			if (iterator.State == PagingState.Complete)			
				return iterator.Deltalink;
			else				
				return null;
		}

		public async Task<string> HandleActualDriveItemsAsync(Action<MediaItem> handleDelegate, CancellationToken cancel)
		{
			//var childrenRequest = _client.Drives["Me"].Items["Root"].Delta;
			var childrenRequest = _client.Drives["Me"].Items["Root"].Delta;
			var items = await childrenRequest.GetAsDeltaGetResponseAsync<DriveDeltaItems>(r => r.QueryParameters.Select = new[]
				{
					"id",
					"name",
					"description",
					"createdDateTime",
					"lastModifiedDateTime",
					"createdBy",
					"lastModifiedBy",
					"parentReference",
					"file",
					"image",
					"video",
					"photo",
					"folder",
					"size",
					"webUrl",
					"deleted",
					"fileSystemInfo"
				});

			PageIterator<DriveItem, DriveDeltaItems> iterator = null;

			int varo = 0;
			
			iterator = PageIterator<DriveItem, DriveDeltaItems>.CreatePageIterator(_client.Adapter, items, callback: 
				item =>
					{
						varo++;
						if (item.Folder != null)
							return true;
				
						var itemInfo = CreateItem(item);
						if (itemInfo != null)
							handleDelegate(itemInfo);
						return true;
					}
			);

			await iterator.IterateAsync(cancel);
			
			if (iterator.State == PagingState.Complete)			
				return iterator.Deltalink;
			else				
				return null;
		}

		MediaItem CreateItem(DriveItem item)
		{
			return item.ToMediaItem();
		}
		

		public Uri CreateAuthorizeLink(object stateIdentifier)
		{
			UriBuilder builder = new UriBuilder(_azure.AuthPath);
			var query = HttpUtility.ParseQueryString(builder.Query);
			query["scope"] = _azure.Scopes;
			query["client_id"] = _azure.ClientId;
			query["response_type"] = "code";
			query["redirect_uri"] = _azure.RedirectUri + _azure.CallbackPath;
			query["prompt"] = "select_account";
			query["state"] = stateIdentifier.ToString();
			builder.Query = query.ToString();
			return builder.Uri;
		}

		public async Task<Result<AccessInformation>> RefreshUserAccessTokenAsync(UserId userId, AccessInformation currentAccess, CancellationToken cancel)
		{
			var response = await _tokenService.RefreshCredentials(_azure, currentAccess.RefreshToken, cancel).ConfigureAwait(false);
			if (response.IsSuccessful)
			{
				var updated = response.Content;
				updated.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(updated.ExpiresIn);
				await _accessStore.SaveAsync(userId, updated, cancel).ConfigureAwait(false);
				return Result.Ok(updated);
			}
			return Result.Fail<AccessInformation>(response.Error.Content);
		}
	}
}
