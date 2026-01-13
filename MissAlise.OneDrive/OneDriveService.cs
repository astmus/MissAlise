using System.Collections;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using Azure.Core;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

//using Microsoft.Graph;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Http.HttpClientLibrary;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.Authentication;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Auth;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using Newtonsoft.Json.Linq;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.OneDrive
{
	internal class OneDriveService : IOneDriveService
	{
		private readonly AzureAd _azure;
		private readonly IHandleContext _ctx;
		private readonly ApiClient _client;
		private readonly IOneDriveTokenService _tokenService;
		private readonly IAuthenticationService _manager;

		public OneDriveService(IOptions<AzureAd> azure, ApiClient client, IHandleContext ctx, IOneDriveTokenService tokenService, IAuthenticationService manager)
		{
			_azure = azure.Value;
			_ctx = ctx;
			_client = client;
			_tokenService = tokenService;
			_manager = manager;
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

		public async Task<string> HandleDeltaDriveItemsAsync(Action<ItemInfo> handleDelegate, CancellationToken cancel)
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
				
						var itemInfo = CreateItemIinfo(item);
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

		public async Task<string> HandleActualDriveItemsAsync(Action<ItemInfo> handleDelegate, CancellationToken cancel)
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
				
						var itemInfo = CreateItemIinfo(item);
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

		ItemInfo CreateItemIinfo(DriveItem item)
		{
			if (item.Deleted != null)							
				return new ItemInfo() { Id = item.Id };

			try
			{
				var jObj = JObject.FromObject(item);

				var resObj = jObj.ToObject<ItemInfo>();
				resObj.Parent = jObj.SelectToken("ParentReference").ToObject<ParentInfo>();
				resObj.Sha256Hash = item.File?.Hashes?.Sha256Hash;
				return resObj;
			}
			catch (Exception)
			{
				return null;
			}
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

		public async Task<Result<AppUser>> RefreshUserAccessTokenAsync(AppUser user, CancellationToken cancel)
		{
			var response = await _tokenService.RefreshCredentials(_azure, user.AccessData.RefreshToken, cancel).ConfigureAwait(false);
			if (response.IsSuccessful)
			{
				user.AccessData = response.Content;
				user.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
				var result = await _manager.UpdateUserAsync(user).ConfigureAwait(false);
				return user;
			}
			else
				return Result.Fail<AppUser>(response.Error.Content);
		}
	}
}
