using MissAlise.ValueObjects.Media;

namespace MissAlise.Entities.Media;

public class Audio : MediaItem
{
	public string? TrackTitle { get; set; }

	public int? Track { get; set; }	

	public string? Genre { get; set; }

	public long? Bitrate { get; set; }

	public string? Album { get; set; }

	public string? Artist { get; set; }

	public int? Disc { get; set; }

	public int? TrackCount { get; set; }

	public int? Year { get; set; }
}
