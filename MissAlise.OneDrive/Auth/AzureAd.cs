namespace MissAlise.OneDrive.Auth;

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
}
