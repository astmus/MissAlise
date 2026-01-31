using System.ComponentModel.DataAnnotations.Schema;
using MissAlise.ValueObjects.Media;

namespace MissAlise.Entities.Media;

public partial class Photo : MediaItem
{
	public string? Cameramake { get; set; }

	public string? Cameramodel { get; set; }

	public double? Exposuredenominator { get; set; }

	public double? Exposurenumerator { get; set; }

	public double? Fnumber { get; set; }

	public double? Focallength { get; set; }

	public int? Iso { get; set; }

	public int? Orientation { get; set; }

	public DateTimeOffset? Takendatetime { get; set; }
	public int? Height { get; set; }

	public int? Width { get; set; }
}

public partial class Image
{
	[NotMapped]
	public IDictionary<string, object> AdditionalData { get; set; }
	public int? Height { get; set; }
	string OdataType { get; set; }
	public int? Width { get; set; }
}
