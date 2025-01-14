using System.ComponentModel.DataAnnotations;

namespace MissAlise.Entities.OneDrive
{
	public class User
	{
		[MaxLength(32)]
		public string Id { get; set; }
		[MaxLength(128)]
		public string DisplayName { get; set; }
		[MaxLength(64)]
		public string GivenName { get; set; }
		[MaxLength(64)]
		public string Mail { get; set; }
		[MaxLength(8)]
		public string PreferredLanguage { get; set; }
		[MaxLength(64)]
		public string Surname { get; set; }
	}
}
