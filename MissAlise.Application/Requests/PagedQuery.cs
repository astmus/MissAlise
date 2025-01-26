namespace MissAlise.Application.Requests
{
	/// <summary>
	/// Параметры пейджинга
	/// </summary>
	public class PagedQuery
	{
		/// <summary>
		/// Номер страницы, должен быть положительным
		/// </summary>
		public int Page { get; set; } = 0;
		/// <summary>
		/// Количество элементов на странице (по умолчанию 32)
		/// </summary>
		public int PerPage { get; set; } = 32;
	}
}
