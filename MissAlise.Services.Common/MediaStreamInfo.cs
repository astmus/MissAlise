using System.Text.Json.Serialization;

public class MediaStreamInfo
{
	[JsonPropertyName("index")]
	public int Index { get; set; }

	[JsonPropertyName("codec_name")]
	public string CodecName { get; set; }

	[JsonPropertyName("codec_long_name")]
	public string CodecLongName { get; set; }

	[JsonPropertyName("profile")]
	public string Profile { get; set; }

	[JsonPropertyName("codec_type")]
	public string CodecType { get; set; }

	[JsonPropertyName("codec_time_base")]
	public string CodecTimeBase { get; set; }

	[JsonPropertyName("codec_tag_string")]
	public string CodecTagString { get; set; }

	[JsonPropertyName("codec_tag")]
	public string CodecTag { get; set; }

	[JsonPropertyName("width")]
	public int Width { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }

	[JsonPropertyName("coded_width")]
	public int CodedWidth { get; set; }

	[JsonPropertyName("coded_height")]
	public int CodedHeight { get; set; }

	[JsonPropertyName("has_b_frames")]
	public int HasBFrames { get; set; }

	[JsonPropertyName("sample_aspect_ratio")]
	public string SampleAspectRatio { get; set; }

	[JsonPropertyName("display_aspect_ratio")]
	public string DisplayAspectRatio { get; set; }

	[JsonPropertyName("pix_fmt")]
	public string PixFmt { get; set; }

	[JsonPropertyName("level")]
	public int Level { get; set; }

	[JsonPropertyName("color_range")]
	public string ColorRange { get; set; }

	[JsonPropertyName("color_space")]
	public string ColorSpace { get; set; }

	[JsonPropertyName("color_transfer")]
	public string ColorTransfer { get; set; }

	[JsonPropertyName("color_primaries")]
	public string ColorPrimaries { get; set; }

	[JsonPropertyName("chroma_location")]
	public string ChromaLocation { get; set; }

	[JsonPropertyName("field_order")]
	public string FieldOrder { get; set; }

	[JsonPropertyName("refs")]
	public int Refs { get; set; }

	[JsonPropertyName("r_frame_rate")]
	public string RFrameRate { get; set; }

	[JsonPropertyName("avg_frame_rate")]
	public string AvgFrameRate { get; set; }

	[JsonPropertyName("time_base")]
	public string TimeBase { get; set; }

	[JsonPropertyName("start_pts")]
	public long StartPts { get; set; }

	[JsonPropertyName("start_time")]
	public string StartTime { get; set; }

	[JsonPropertyName("duration_ts")]
	public long DurationTs { get; set; }

	[JsonPropertyName("duration")]
	public string Duration { get; set; }

	[JsonPropertyName("bit_rate")]
	public string BitRate { get; set; }

	[JsonPropertyName("nb_frames")]
	public string NbFrames { get; set; }
}
