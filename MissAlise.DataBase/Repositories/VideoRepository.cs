using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Contexts;
using MissAlise.DataBase.Mapping;
using MissAlise.DataBase.Models;
using MissAlise.Entities.Media;
using MissAlise.Interfaces;

namespace MissAlise.DataBase.Repositories
{
	internal class VideoRepository : ItemsRepository, IVideoRepository
	{
		private readonly ILogger<VideoRepository> logger;

		public VideoRepository(UserMediaContext ctx, ILogger<VideoRepository> logger)
		{
			this.ctx = ctx;
			this.logger = logger;
		}

		public async Task<IItemsPage<Video>> GetPageAsync(int page, int perPage, CancellationToken cancel)
		{
			var pageDb = await ctx.Videos.AsNoTracking().LoadPageOfItemsAsync<DbVideo>(page, perPage, cancel);
			var items = pageDb.Select(MediaDbMapper.ToVideo).ToArray();
			return new Application.Common.PaginatedList<Video>(items, pageDb.TotalCount, page, perPage);
		}

		public async Task<Video?> FirstAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Videos.AsNoTracking().ToListAsync(cancel);
			return list.Select(MediaDbMapper.ToVideo).FirstOrDefault(predicate.Compile());
		}

		public async Task DeleteAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Videos.ToListAsync(cancel);
			var toRemove = list.Where(db => predicate.Compile()(MediaDbMapper.ToVideo(db))).ToList();
			ctx.Videos.RemoveRange(toRemove);
			await ctx.SaveChangesAsync(cancel);
		}

		public async Task<bool> IsExistsAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
		{
			var list = await ctx.Videos.AsNoTracking().ToListAsync(cancel);
			return list.Select(MediaDbMapper.ToVideo).Any(predicate.Compile());
		}

		public Task<Video> AddAsync(Video item, CancellationToken cancel)
			=> throw new NotImplementedException();
	}
}
