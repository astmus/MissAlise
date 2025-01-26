using System.Linq.Expressions;
using DnsClient.Internal;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MissAlise.DataBase.Models;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.DataBase
{
	internal class PhotoRepository : ItemsRepository, IPhotoRepository
	{
		private readonly ILogger<PhotoRepository> logger;
		
		public PhotoRepository(UserMediaContext ctx, ILogger<PhotoRepository> logger)
		{
			this.ctx = ctx;
			this.logger = logger;
		}

		public async Task<IItemsPage<Photo>> GetPageAsync(int page, int perPage, CancellationToken cancel)
		{
			return await ctx.Photos.AsNoTracking().LoadPageOfItemsAsync(page, perPage, cancel);
		}

		public Task<bool> IsExistsAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
			=> ctx.Photos.AnyAsyncEF(predicate, cancel);

		public async Task DeleteAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
		{
			try
			{
				var delete = await ctx.Photos.Where(predicate).DeleteAsync(cancel).ConfigureAwait(false);
			}
			catch (Exception error)
			{
				logger.LogError(error, "Delete videos from db error {message}", error.Message);
			}
		}

		public Task<Photo?> FirstAsync(Expression<Func<Photo, bool>> predicate, CancellationToken cancel)
			=> ctx.Photos.AsNoTracking().FirstOrDefaultAsyncEF(predicate, cancel);
	}
}
