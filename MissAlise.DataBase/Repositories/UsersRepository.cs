using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;
using MongoDB.Driver;
using Folder = MissAlise.Entities.OneDrive.Folder;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.DataBase
{
	internal class UsersRepository : IUsersRepository
	{
		private readonly UserMediaContext ctx;
		const string defPath = @"m:\Sync\";
		public UsersRepository(UserMediaContext ctx)
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
						Createddatetime = DateTime.UtcNow,
						Modifiedatetime = DateTime.UtcNow,
						Name = user.DisplayName,
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
		public IOneDriveUser Owner { get; internal set; }
		public Folder RootFolder { get; set; }
		public Func<Task<int>> SaveAsync { get; internal set; }
	}
}
