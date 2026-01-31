using Microsoft.EntityFrameworkCore;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.DataBase.Contexts;
using MissAlise.Interfaces;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Media;
using MongoDB.Driver;

namespace MissAlise.DataBase.Repositories
{
	internal class UserRepository : IUserRepository, IUserStorage
	{
		private readonly UserMediaContext ctx;
		private readonly IHandleContext handleCtx;
		const string defPath = @"m:\Sync\";

		public UserId? Owner { get; internal set; }
		public MediaPath RootFolder { get; set; } = null!;
		public Func<Task<int>> SaveAsync { get; internal set; } = null!;

		public UserRepository(UserMediaContext ctx, IHandleContext handleCtx)
		{
			this.ctx = ctx;
			this.handleCtx = handleCtx;
		}

		public async Task<IUserStorage> GetStorageAsync(UserId user, CancellationToken cancel)
		{
			//if (await ctx.Users.FirstOrDefaultAsync(u => u.Id == user.Id) is not User owner)
			//{
			//	owner = new User()
			//	{
			//		Id = user.Id,
			//		DisplayName = user.DisplayName,
			//		GivenName = user.GivenName,
			//		Mail = user.Mail,
			//		PreferredLanguage = user.PreferredLanguage,
			//		Surname = user.Surname,
			//		UserPrincipalName = string.Empty
			//	};

			//	var result = await ctx.Users.AddAsync(owner, cancel);
			//	await ctx.SaveChangesAsync(cancel).ConfigureAwait(false);
			//}
			//var userStroage = await ctx.Folders.Include(u => u.Children).AsSplitQuery().FirstOrDefaultAsync(u => u.Id.ToString() == user.ToString()).ConfigureAwait(false);

			await Task.Delay(10);

			SaveAsync = () => this.ctx.SaveChangesAsync(cancel);
			return this;
		}

		//public async Task<IUserStorage?> LoadForCurrentUserAsync(UserId user, CancellationToken cancel)
		//{
		//	if (handleCtx.CurrentUser is not AppUser appUser)
		//		return null;

		//	if (await ctx.Folders.Include(u => u.Children).AsSplitQuery().FirstOrDefaultAsync(u => u.Title == appUser.UserName).ConfigureAwait(false) is Folder userFolder)
		//		RootFolder = userFolder;
		//	else
		//	{
		//		RootFolder = new Folder()
		//		{
		//			Id = $"{appUser.Id} {appUser.UserName}",
		//			Title = appUser.UserName,
		//			CreatedDateTime = DateTimeOffset.UtcNow,
		//			ModifieDateTime = DateTimeOffset.UtcNow,
		//			Path = Path.Combine(defPath, appUser.UserName ?? appUser.Id)
		//		};
		//		await ctx.Folders.AddAsync(RootFolder, cancel);
		//		await ctx.SaveChangesAsync(cancel);
		//	}
		//	SaveAsync = () => ctx.SaveChangesAsync(cancel);
		//	return this;
		//}
	}
}
