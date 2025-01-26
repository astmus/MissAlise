using System.Text.Json.Serialization;

public class MediaInfo
{
	[JsonPropertyName("streams")]
	public List<MediaStreamInfo> Streams { get; set; }

	[JsonPropertyName("format")]
	public MediaFormatInfo Format { get; set; }
}
