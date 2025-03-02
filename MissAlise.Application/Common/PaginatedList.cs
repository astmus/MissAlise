using MissAlise.Interfaces;

namespace MissAlise.Application.Common
{
	public class PaginatedList<T> : List<T>, IItemsPage<T>
	{
		public int Page { get; private set; }
		public int PerPage { get; private set; }
		public int TotalCount { get; private set; }
		public int TotalPages { get; private set; }
		public bool HasPreviousPage => Page > 0;
		public bool HasNextPage => Page < TotalPages;

		public PaginatedList(IEnumerable<T> items, int count, int page, int perPage)
		{
			Page = page;
			TotalPages = (int)Math.Ceiling(count / (double)perPage);
			TotalCount = count;
			PerPage = perPage;
			AddRange(items);
		}
	}
}
