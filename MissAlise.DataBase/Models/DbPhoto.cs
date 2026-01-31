using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.DataBase.Models;

[Table("Photos")]
public class DbPhoto : DbMediaItem
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
}
