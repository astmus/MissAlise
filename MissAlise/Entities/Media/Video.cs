using MissAlise.ValueObjects.Media;

namespace MissAlise.Entities.Media;

public partial class Video : MediaItem
{
	public int? Audiobitspersample { get; set; }

	public int? Audiochannels { get; set; }

	public string? Audioformat { get; set; }

	public int? Audiosamplespersecond { get; set; }

	public int? Bitrate { get; set; }	

	public string? Fourcc { get; set; }

	public double? Framerate { get; set; }

	public int? Height { get; set; }

	public int? Width { get; set; }
}
