using MissAlise.Entities.OneDrive;
using Refit;

namespace MissAlise.Interfaces
{
	public interface IOneDriveApi
	{
		Task<ApiResponse<Credentials>> GetCredentialsByCode(AzureAd azure, string code, CancellationToken cancel)
		{
			var content = new
			{
				client_id = azure.ClientId,
				redirect_uri = azure.RedirectUri + azure.CallbackPath,
				client_secret = azure.ClientSecret,
				code = code,
				grant_type = "authorization_code"
			};
			return GetCredentialsByCode(content, cancel);
		}

		[Post("/common/oauth2/v2.0/token")]
		internal Task<ApiResponse<Credentials>> GetCredentialsByCode([Body(BodySerializationMethod.UrlEncoded)] object body, CancellationToken cancel);
	}
} 
