using System.Web;

namespace MissAlise.Entities.OneDrive;

public class AzureAd
{
	public string Instance { get; set; }
	public string TenantId { get; set; }
	public string ClientId { get; set; }
	public string ClientSecret { get; set; }
	public string CallbackPath { get; set; }
	public string RedirectUri { get; set; }
	public string Scopes { get; set; }
	public string AuthPath { get; set; }
	public string TokenPath { get; set; }

	public Uri AuthorizeLink(string stateIdentifier)
	{
		UriBuilder b = new UriBuilder(AuthPath);
		var query = HttpUtility.ParseQueryString(b.Query);
		query["scope"] = Scopes;
		query["client_id"] = ClientId;
		query["response_type"] = "code";
		query["redirect_uri"] = RedirectUri + CallbackPath;
		query["prompt"] = "select_account";
		query["state"] = stateIdentifier;
		b.Query = query.ToString();
		return b.Uri;
	}
}
