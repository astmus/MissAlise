using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.DataBase.Repositories
{
	internal class PhotoRepository : ItemsRepository, IPhotoRepository
	{
		public PhotoRepository(UserMediaContext ctx)
		{
			this.ctx = ctx;
		}

		public async Task<Photo> AddAsync(Photo item, CancellationToken cancel)
		{
			var result = await ctx.Photos.AddAsync(item, cancel);
			return result.Entity;
		}

		public async Task<IItemsPage<Photo>> GetPageAsync(int page, int perPage, CancellationToken cancel)
			=> await ctx.Photos.AsNoTracking().LoadPageOfItemsAsync(page, perPage, cancel);

		public async Task<bool> IsExistsAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
			=> await ctx.Photos.AnyAsyncEF(predicate, cancel);

		public async Task DeleteAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
			=> await ctx.Photos.Where(predicate).DeleteAsync(cancel);

		public async Task<Photo?> FirstAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
			=> await ctx.Photos.AsNoTracking().FirstOrDefaultAsyncEF(predicate, cancel);
	}
}
