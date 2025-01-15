using System.Web;
using Microsoft.Graph;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.OneDrive
{
	public class OneDriveService : IRemoteStorageService
	{
		private readonly AzureAd azureOptions;
		private readonly GraphServiceClient client;

		public OneDriveService(AzureAd azureOptions, GraphServiceClient client)
		{
			this.azureOptions = azureOptions;
			this.client = client;
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
			var me = await client.Me.GetAsync(cancellationToken: cancel);
			if (me != null)
				return new User() { Id = me.Id, DisplayName = me.DisplayName, GivenName = me.GivenName, Mail = me.Mail, PreferredLanguage = me.PreferredLanguage, Surname = me.Surname };

			return null;
		}
	}
}
