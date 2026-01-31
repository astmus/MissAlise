namespace MissAlise.DataBase.Mapping;

using MissAlise.DataBase.Models;
using MissAlise.Entities.Media;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

internal static class MediaDbMapper
{
	public static DbMediaItem ToDb(MediaItem item) => new()
	{
		Id = item.Id.Value,
		Kind = item.Kind,
		Provider = item.Provider,
		RemoteItemId = item.RemoteItemId,
		DriveId = item.DriveId,
		Name = item.Name,
		ParentPath = item.Path.Value,
		MimeType = item.MimeType.ToString(),
		SizeBytes = item.Size.Bytes,
		CreatedAt = item.Timestamps.CreatedAt,
		ModifiedAt = item.Timestamps.ModifiedAt,
		TakenAt = item.Timestamps.TakenAt,
		IsDeleted = item.IsDeleted,
		Width = item.Dimensions?.Width,
		Height = item.Dimensions?.Height,
		DurationSeconds = item.Duration?.Value.TotalSeconds,
		HashAlgorithm = item.Hash?.Algorithm,
		HashValue = item.Hash?.Value,
	};

	public static void ApplyToExisting(DbMediaItem db, MediaItem item)
	{
		db.Kind = item.Kind;
		db.Provider = item.Provider;
		db.RemoteItemId = item.RemoteItemId;
		db.DriveId = item.DriveId;

		db.Name = item.Name;
		db.ParentPath = item.Path.Value;
		db.MimeType = item.MimeType.Value;
		db.SizeBytes = item.Size.Bytes;

		db.CreatedAt = item.Timestamps.CreatedAt;
		db.ModifiedAt = item.Timestamps.ModifiedAt;
		db.TakenAt = item.Timestamps.TakenAt;

		db.IsDeleted = item.IsDeleted;

		db.Width = item.Dimensions?.Width;
		db.Height = item.Dimensions?.Height;
		db.DurationSeconds = item.Duration?.Value.TotalSeconds;

		db.HashAlgorithm = item.Hash?.Algorithm;
		db.HashValue = item.Hash?.Value;
	}

	public static MediaItem ToDomain(DbMediaItem db) => new(
		kind: (MediaKind)db.Kind,
		provider: (ProviderKind)db.Provider,
		remoteItemId: db.RemoteItemId,
		driveId: db.DriveId,
		name: db.Name,
		path: MediaPath.FromNullable(db.ParentPath),
		size: new FileSize(db.SizeBytes),
		mimeType: new MimeType(db.MimeType),
		timestamps: new MediaTimestamps(db.CreatedAt, db.ModifiedAt, db.TakenAt),
		isDeleted: db.IsDeleted,
		dimensions: (db.Width is > 0 && db.Height is > 0) ? new Dimensions(db.Width!.Value, db.Height!.Value) : null,
		duration: (db.DurationSeconds is > 0) ? Duration.FromSeconds(db.DurationSeconds!.Value) : null,
		hash: (!string.IsNullOrWhiteSpace(db.HashAlgorithm) && !string.IsNullOrWhiteSpace(db.HashValue))
			? new ContentHash(db.HashAlgorithm!, db.HashValue!)
			: null
	)
	{
		Id = new MediaItemId(db.Id)
	};

	public static DbPhoto ToDb(Photo item)
	{
		var db = new DbPhoto { Id = item.Id.Value };
		ApplyToExisting(db, item);
		db.Cameramake = item.Cameramake;
		db.Cameramodel = item.Cameramodel;
		db.Exposuredenominator = item.Exposuredenominator;
		db.Exposurenumerator = item.Exposurenumerator;
		db.Fnumber = item.Fnumber;
		db.Focallength = item.Focallength;
		db.Iso = item.Iso;
		db.Orientation = item.Orientation;
		db.Takendatetime = item.Takendatetime;
		db.Width = item.Width;
		db.Height = item.Height;
		return db;
	}

	public static Photo ToPhoto(DbPhoto db)
	{
		var m = ToDomain(db);
		var photo = new Photo
		{
			Id = m.Id,
			Cameramake = db.Cameramake,
			Cameramodel = db.Cameramodel,
			Exposuredenominator = db.Exposuredenominator,
			Exposurenumerator = db.Exposurenumerator,
			Fnumber = db.Fnumber,
			Focallength = db.Focallength,
			Iso = db.Iso,
			Orientation = db.Orientation,
			Takendatetime = db.Takendatetime,
			Width = db.Width,
			Height = db.Height
		};
		photo.ApplyFrom(m);
		return photo;
	}

	public static DbVideo ToDb(Video item)
	{
		var db = new DbVideo { Id = item.Id.Value };
		ApplyToExisting(db, item);
		db.Audiobitspersample = item.Audiobitspersample;
		db.Width = item.Width;
		db.Height = item.Height;
		db.Audiochannels = item.Audiochannels;
		db.Audioformat = item.Audioformat;
		db.Audiosamplespersecond = item.Audiosamplespersecond;
		db.Bitrate = item.Bitrate;
		db.Fourcc = item.Fourcc;
		db.Framerate = item.Framerate;
		return db;
	}

	public static Video ToVideo(DbVideo db)
	{
		var m = ToDomain(db);
		var video = new Video
		{
			Id = m.Id,
			Audiobitspersample = db.Audiobitspersample,
			Audiochannels = db.Audiochannels,
			Audioformat = db.Audioformat,
			Audiosamplespersecond = db.Audiosamplespersecond,
			Bitrate = db.Bitrate,
			Fourcc = db.Fourcc,
			Framerate = db.Framerate,
			Width = db.Width,
			Height = db.Height
		};
		video.ApplyFrom(m);
		return video;
	}
}
