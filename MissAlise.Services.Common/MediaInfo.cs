using System.Text.Json.Serialization;

public class MediaInfo
{
	[JsonPropertyName("streams")]
	public List<MediaStreamInfo> Streams { get; set; }

	[JsonPropertyName("format")]
	public MediaFormatInfo Format { get; set; }

	[JsonPropertyName("anomaly")]
	public string? Anomaly { get; set; }
}

public enum ImageFormat
{
	jpeg, png, bmp, tiff, webp
}

public enum VideoFormat
{
	mp4, mov, avi, mkv, webm
}

public class ApplyVideoParams
{
	public string Name { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public VideoFormat? Format { get; set; }
}

public class ApplyImageParams
{
	public string Name { get; set; }	
	public int? Width { get; set; }	
	public int? Height { get; set; }
	public ImageFormat? Format { get; set; }	
}