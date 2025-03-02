namespace MissAlise.Application.Dto
{
	public class PhotoSlim
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class Photo
	{
		public string Id { get; set; }
		public string? Name { get; set; }
		public int? Size { get; set; }
		public string? MimeType { get; set; }
		public DateTime? TakenDateTime { get; set; }
		public int? Iso { get; set; }
		public int? Height { get; set; }
		public int? Width { get; set; }
	}
}
