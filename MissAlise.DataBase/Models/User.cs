using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissAlise.DataBase.Models
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
		[MaxLength(64)]
		public string UserPrincipalName { get; set; }

		public Folder? StorageFolder { get; set; }
	}
}
