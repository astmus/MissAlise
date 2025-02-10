using System.Web;
using Microsoft.Extensions.Options;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
namespace MissAlise.OneDrive
{
	public class OneDriveService : IOneDriveService
	{
		private readonly AzureAd azureOptions;
		//private readonly GraphServiceClient client;
		private readonly IOneDriveClient oneClient;

		public OneDriveService(IOptions<AzureAd> azureOptions, /*GraphServiceClient client,*/ IOneDriveClient oneClient)
		{
			this.azureOptions = azureOptions.Value;
			//this.client = client;
			this.oneClient = oneClient;			
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
			var items = await oneClient.RootItems(cancel);
			//var me = await client.Me.GetAsync(cancellationToken: cancel);
			//if (me != null)
			//	return new User() { Id = me.Id, DisplayName = me.DisplayName, GivenName = me.GivenName, Mail = me.Mail, PreferredLanguage = me.PreferredLanguage, Surname = me.Surname };

			return null;
		}

		public async Task<DeltaGetResponse> GetRootItems(CancellationToken cancel)
		{
			//var resp = await client.Drives["ff"].Items[""].Delta.GetAsDeltaGetResponseAsync();
			var items = await oneClient.RootItems(cancel);
			var res =  await oneClient.RootDelta(cancel);
			return res.Content;
		}
	}
}
