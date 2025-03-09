using Microsoft.EntityFrameworkCore;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.DataBase.Contexts;
using MissAlise.Interfaces;
using MongoDB.Driver;
using Folder = MissAlise.Entities.OneDrive.Folder;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.DataBase.Repositories
{
	internal class UserRepository : IUserRepository, IUserStorage
	{
		private readonly UserMediaContext ctx;
		private readonly IHandleContext handleCtx;
		const string defPath = @"m:\Sync\";

		public User? Owner { get; internal set; }
		public Folder RootFolder { get; set; }
		public Func<Task<int>> SaveAsync { get; internal set; }

		public UserRepository(UserMediaContext ctx, IHandleContext handleCtx)
		{
			this.ctx = ctx;
			this.handleCtx = handleCtx;
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

			SaveAsync = () => this.ctx.SaveChangesAsync(cancel);
			return this;
		}

		public async Task<IUserStorage?> LoadForCurrentUserAsync(CancellationToken cancel)
		{
			if (handleCtx.CurrentUser is not AppUser appUser)
				return null;

			if (await ctx.Folders.Include(u => u.Children).AsSplitQuery().FirstOrDefaultAsync(u => u.Title == appUser.UserName).ConfigureAwait(false) is Folder userFolder)
				RootFolder = userFolder;
			else
			{
				RootFolder = new Folder()
				{
					Id = $"{appUser.Id} {appUser.UserName}",
					Title = appUser.UserName,
					CreatedDateTime = DateTimeOffset.UtcNow,
					ModifieDateTime = DateTimeOffset.UtcNow,
					Path = Path.Combine(defPath, appUser.UserName ?? appUser.Id)
				};
				await ctx.Folders.AddAsync(RootFolder, cancel);
				await ctx.SaveChangesAsync(cancel);
			}
			SaveAsync = () => ctx.SaveChangesAsync(cancel);
			return this;
		}
	}
}
