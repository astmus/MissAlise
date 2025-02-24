using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using MissAlise.Entities.OneDrive;

namespace MissAlise.Application.Models
{
	public record Claimant(string Id, string Name);
	public class AppUser : IdentityUser
	{
		public AccessInformation? AccessData { get; set; }
	}
}
