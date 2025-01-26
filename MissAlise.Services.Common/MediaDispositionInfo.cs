using System.Text.Json.Serialization;

public class MediaDispositionInfo
{
	[JsonPropertyName("default")]
	public int Default { get; set; }

	[JsonPropertyName("dub")]
	public int Dub { get; set; }

	[JsonPropertyName("original")]
	public int Original { get; set; }

	[JsonPropertyName("comment")]
	public int Comment { get; set; }

	[JsonPropertyName("lyrics")]
	public int Lyrics { get; set; }

	[JsonPropertyName("karaoke")]
	public int Karaoke { get; set; }

	[JsonPropertyName("forced")]
	public int Forced { get; set; }

	[JsonPropertyName("hearing_impaired")]
	public int HearingImpaired { get; set; }

	[JsonPropertyName("visual_impaired")]
	public int VisualImpaired { get; set; }

	[JsonPropertyName("clean_effects")]
	public int CleanEffects { get; set; }

	[JsonPropertyName("attached_pic")]
	public int AttachedPic { get; set; }

	[JsonPropertyName("timed_thumbnails")]
	public int TimedThumbnails { get; set; }
}
