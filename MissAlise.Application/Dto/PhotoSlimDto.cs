namespace MissAlise.Application.Dto
{
	public class PhotoSlimDto
	{
		public string Id { get; set; }		
		public string Name { get; set; }
	}

	public class PhotoDto
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

	public class PhotoExtDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}
