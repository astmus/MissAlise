using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.DataBase.Models;

[Table("Audios")]
public class DbAudio : DbMediaItem
{
	[Column(TypeName = "varchar(256)")]
	public string? TrackTitle { get; set; }
	public int? Track { get; set; }
	[Column(TypeName = "varchar(64)")]
	public string? Genre { get; set; }
	public long? Bitrate { get; set; }
	[Column(TypeName = "varchar(256)")]
	public string? Album { get; set; }
	[Column(TypeName = "varchar(256)")]
	public string? Artist { get; set; }
	public int? Disc { get; set; }
	public int? TrackCount { get; set; }
	public int? Year { get; set; }
}
