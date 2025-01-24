namespace MissAlise.Entities.OneDrive
{
	public interface IOneDriveUser
	{
		string DisplayName { get; set; }
		string GivenName { get; set; }
		string Id { get; set; }
		string Mail { get; set; }
		string PreferredLanguage { get; set; }
		string Surname { get; set; }
	}
}