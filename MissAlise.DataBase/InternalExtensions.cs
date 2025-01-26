using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MissAlise.Application.Common;
using MissAlise.Background;
using MissAlise.DataBase.Models;
using MissAlise.Interfaces;
using MongoDB.Driver;

namespace MissAlise.DataBase
{
	internal static class InternalExtensions
	{
		public static async Task<IItemsPage<T>> LoadPageOfItemsAsync<T>(this IQueryable<T> query, int page, int perPage, CancellationToken cancel) where T : class
		{
			var count = await query.CountAsync();
			var items = await query.Skip(page * perPage).Take(perPage).ToListAsync(cancel);
			return new PaginatedList<T>(items, count, page, perPage);
		}
	}
}
