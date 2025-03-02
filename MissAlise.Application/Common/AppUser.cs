using Microsoft.AspNetCore.Identity;
using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Common
{
	public record Claimant(string Id, string Name);
	public class AppUser : IdentityUser
	{
		public virtual AccessInformation? AccessData { get; set; }
	}
}
