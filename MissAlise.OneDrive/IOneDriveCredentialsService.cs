using Microsoft.Graph.Models;
using MissAlise.Entities.OneDrive;
using Refit;

namespace MissAlise.OneDrive
{

	public interface IOneDriveCredentialsService
	{
		Task<ApiResponse<Credentials>> GetCredentialsByCode(AzureAd azure, string code, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				redirect_uri = azure.RedirectUri + azure.CallbackPath,
				client_secret = azure.ClientSecret,
				code,
				grant_type = "authorization_code"
			};
			return RequestCredentials(content, cancel);
		}

		Task<ApiResponse<Credentials>> RefreshCredentials(AzureAd azure, string refreshToken, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				client_secret = azure.ClientSecret,
				refresh_token = refreshToken,
				grant_type = "refresh_token"
			};
			return RequestCredentials(content, cancel);
		}

		[Post("/common/oauth2/v2.0/token")]
		internal Task<ApiResponse<Credentials>> RequestCredentials([Body(BodySerializationMethod.UrlEncoded)] object body, CancellationToken cancel);
	}
}
