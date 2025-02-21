namespace MissAlise.Entities.OneDrive;

public partial class Photo : BaseFile
{
	public string? Cameramake { get; set; }

	public string? Cameramodel { get; set; }

	public double? Exposuredenominator { get; set; }

	public double? Exposurenumerator { get; set; }

	public double? Fnumber { get; set; }

	public double? Focallength { get; set; }

	public int? Iso { get; set; }

	public int? Orientation { get; set; }

	public DateTime? Takendatetime { get; set; }

	public int? Height { get; set; }

	public int? Width { get; set; }
}
