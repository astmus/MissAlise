using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.DataBase.Models;

public partial class Audio : File
{
	public int Audioid { get; set; }

	//public override int Fileid { get; set; }

	public string? TrackTitle { get; set; }

	public int? Track { get; set; }

	public long? Duration { get; set; }

	public string? Genre { get; set; }

	public long? Bitrate { get; set; }

	public string? Album { get; set; }

	public string? Artist { get; set; }

	public int? Disc { get; set; }

	public int? Trackcount { get; set; }

	public int? Year { get; set; }


	// public virtual File? File { get; set; }
}
