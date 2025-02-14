using MissAlise.Entities.OneDrive;
using Refit;

namespace MissAlise.OneDrive
{

	public interface IOneDriveCredentialsService
	{
		async Task<ApiResponse<UserCredentials>> GetCredentialsByCode(AzureAd azure, string code, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				redirect_uri = azure.RedirectUri + azure.CallbackPath,
				client_secret = azure.ClientSecret,
				code,
				grant_type = "authorization_code"
			};

			return await RequestCredentials(content, cancel);
		}

		Task<ApiResponse<string>> GetCredentialsByCode2(AzureAd azure, string code, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				redirect_uri = azure.RedirectUri + azure.CallbackPath,
				client_secret = azure.ClientSecret,
				code,
				grant_type = "authorization_code"
			};
			return RequestCredentials2(content, cancel);
		}

		async Task<ApiResponse<UserCredentials>> RefreshCredentials(AzureAd azure, string refreshToken, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				client_secret = azure.ClientSecret,
				refresh_token = refreshToken,
				grant_type = "refresh_token"
			};
			return await RequestCredentials(content, cancel);
		}

		[Post("/common/oauth2/v2.0/token")]
		internal Task<ApiResponse<string>> RequestCredentials2([Body(BodySerializationMethod.UrlEncoded)] object body, CancellationToken cancel);

		[Post("/common/oauth2/v2.0/token")]
		internal Task<ApiResponse<UserCredentials>> RequestCredentials([Body(BodySerializationMethod.UrlEncoded)] object body, CancellationToken cancel);
	}
}
