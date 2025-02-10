using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Models;
using MissAlise.Interfaces;
using MongoDB.Driver;
using Folder = MissAlise.Entities.OneDrive.Folder;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.DataBase
{
	internal class UserRepository : IUserRepository
	{
		private readonly UserMediaContext ctx;
		const string defPath = @"m:\Sync\";
		public UserRepository(UserMediaContext ctx)
		{
			this.ctx = ctx;
		}

		public async Task<IUserStorage> GetUserStorageAsync(User user, CancellationToken cancel)
		{
			if (await ctx.Users.FirstOrDefaultAsync(u => u.Id == user.Id) is not User owner)
			{
				owner = new User()
				{
					Id = user.Id,
					DisplayName = user.DisplayName,
					GivenName = user.GivenName,
					Mail = user.Mail,
					PreferredLanguage = user.PreferredLanguage,
					Surname = user.Surname,
					UserPrincipalName = string.Empty,
					StorageFolder = new Folder() { 
						Title = user.DisplayName,
						CreatedDateTime = DateTime.UtcNow,
						ModifieDateTime = DateTime.UtcNow,
						Name = user.GivenName,
						Path = Path.Combine(defPath, user.DisplayName)
					}
				};
				var result = await ctx.Users.AddAsync(owner, cancel);
				await ctx.SaveChangesAsync(cancel).ConfigureAwait(false);
			}
			var userStroage = await ctx.Users.Include(u => u.StorageFolder).ThenInclude(u=>u.Folders).ThenInclude(u=>u.Files).AsSplitQuery()
			.FirstOrDefaultAsync(u => u.Id == user.Id).ConfigureAwait(false);

			return new UserStorage()
			{
				Owner = userStroage,
				RootFolder = userStroage.StorageFolder,
				SaveAsync = () => ctx.SaveChangesAsync(cancel)
			};
		}
	}

	internal class UserStorage : IUserStorage
	{
		public required User Owner { get; internal set; }
		public required Folder RootFolder { get; set; }
		public required Func<Task<int>> SaveAsync { get; internal set; }
	}
}
