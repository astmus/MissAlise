namespace MissAlise.Interfaces
{
	public interface IPageInfo
	{
		bool HasNextPage { get; }
		bool HasPreviousPage { get; }
		int Page { get; }
		int PerPage { get; }
		int TotalCount { get; }
		int TotalPages { get; }
	}

	public interface IItemsPage<T> : IPageInfo, IEnumerable<T>
	{

	}
}
