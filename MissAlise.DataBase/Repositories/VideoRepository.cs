using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Contexts;
using MissAlise.Entities.OneDrive;
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
			return await ctx.Videos.AsNoTracking().LoadPageOfItemsAsync(page, perPage, cancel);
		}

		public Task<Video?> FirstAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
			=> ctx.Videos.AsNoTracking().FirstOrDefaultAsyncEF(predicate, cancel);

		public async Task DeleteAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
		{
			await ctx.Videos.Where(predicate).DeleteAsync(cancel).ConfigureAwait(false);
		}

		public async Task<bool> IsExistsAsync(Expression<Func<Video, bool>> predicate, CancellationToken cancel)
			=> await ctx.Videos.AnyAsyncEF(predicate, cancel);
		public Task<Video> AddAsync(Video item, CancellationToken cancel)
			=> throw new NotImplementedException();
	}
}
