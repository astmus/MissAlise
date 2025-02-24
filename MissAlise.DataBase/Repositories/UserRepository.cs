using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.Interfaces;
using MongoDB.Driver;
using Folder = MissAlise.Entities.OneDrive.Folder;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.DataBase.Repositories
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
					UserPrincipalName = string.Empty					
				};
				var result = await ctx.Users.AddAsync(owner, cancel);
				await ctx.SaveChangesAsync(cancel).ConfigureAwait(false);
			}
			var userStroage = await ctx.Folders.Include(u => u.Children).AsSplitQuery().FirstOrDefaultAsync(u => u.Id.ToString() == user.Id).ConfigureAwait(false);

			return new UserStorage()
			{
				RootFolder = new Folder()
				{
					Title = user.DisplayName,
					CreatedDateTime = DateTime.UtcNow,
					ModifieDateTime = DateTime.UtcNow,					
					Path = Path.Combine(defPath, user.DisplayName)
				},
				SaveAsync = () => ctx.SaveChangesAsync(cancel)
			};
		}
	}

	internal class UserStorage : IUserStorage
	{
		public User? Owner { get; internal set; }
		public required Folder RootFolder { get; set; }
		public required Func<Task<int>> SaveAsync { get; internal set; }
	}
}
