namespace MissAlise.WebApi.Auth;

public class AzureConfiguration
{
	public string Instance { get; set; }
	public string TenantId { get; set; }
	public string ClientId { get; set; }
	public string ClientSecret { get; set; }
	public string RedirectUri { get; set; }
	public string Scopes { get; set; }
	public IEnumerable<string> GetScopes()
		=> initialScopes ?? (initialScopes = Scopes?.Split(' '));
	private IEnumerable<string> initialScopes;
}
