using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.DataBase.Models;

[Table("Videos")]
public class DbVideo : DbMediaItem
{
	public int? Audiobitspersample { get; set; }
	public int? Audiochannels { get; set; }
	[Column(TypeName = "varchar(64)")]
	public string? Audioformat { get; set; }
	public int? Audiosamplespersecond { get; set; }
	public int? Bitrate { get; set; }
	[Column(TypeName = "varchar(16)")]
	public string? Fourcc { get; set; }
	public double? Framerate { get; set; }
}
