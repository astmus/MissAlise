using System.Linq.Expressions;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Mapping;
using MissAlise.DataBase.Models;
using MissAlise.Entities.Media;
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
			var db = MediaDbMapper.ToDb(item);
			var result = await ctx.Photos.AddAsync(db, cancel);
			await ctx.SaveChangesAsync(cancel);
			return MediaDbMapper.ToPhoto(result.Entity);
		}

		public async Task<IItemsPage<Photo>> GetPageAsync(int page, int perPage, CancellationToken cancel)
		{
			var pageDb = await ctx.Photos.AsNoTracking().LoadPageOfItemsAsync<DbPhoto>(page, perPage, cancel);
			var items = pageDb.Select(MediaDbMapper.ToPhoto).ToArray();
			return new Application.Common.PaginatedList<Photo>(items, pageDb.TotalCount, page, perPage);
		}

		public async Task<bool> IsExistsAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Photos.AsNoTracking().ToListAsync(cancel);
			return list.Select(MediaDbMapper.ToPhoto).Any(predicate.Compile());
		}

		public async Task DeleteAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Photos.ToListAsync(cancel);
			var toRemove = list.Where(db => predicate.Compile()(MediaDbMapper.ToPhoto(db))).ToList();
			ctx.Photos.RemoveRange(toRemove);
			await ctx.SaveChangesAsync(cancel);
		}

		public async Task<Photo?> FirstAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Photos.AsNoTracking().ToListAsync(cancel);
			return list.Select(MediaDbMapper.ToPhoto).FirstOrDefault(predicate.Compile());
		}

		public new async Task<TItem?> GetById<TItem>(string itemId, CancellationToken cancel) where TItem : MediaItem
		{
			if (typeof(TItem) == typeof(Photo))
			{
				var db = await ctx.Photos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemId, cancel);
				return (db != null ? MediaDbMapper.ToPhoto(db) : null) as TItem;
			}
			return await base.GetById<TItem>(itemId, cancel);
		}
	}
}
