namespace MissAlise.Entities.Media;

using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public class MediaItem
{
    public required MediaItemId Id { get; init; }
    public MediaKind Kind { get; protected set; }
    public ProviderKind Provider { get; private set; }
    public string RemoteItemId { get; private set; }
    public string? DriveId { get; private set; }

    public string Name { get; private set; }
    public MediaPath Path { get; private set; }

    public FileSize Size { get; private set; }
    public MimeType MimeType { get; private set; }
    public ContentHash? Hash { get; private set; }

    public MediaTimestamps Timestamps { get; private set; }

    public Dimensions? Dimensions { get; private set; }  // photo/video
    public Duration? Duration { get; private set; }      // video/audio

    public bool IsDeleted { get; private set; }
	
	public MediaItem()
	{ 
	}

	public MediaItem(		
		MediaKind kind,
		ProviderKind provider,
		string remoteItemId,
		string? driveId,
		string name,
		MediaPath path,
		FileSize size,
		MimeType mimeType,
		MediaTimestamps timestamps,
		ContentHash? hash = null,
		Dimensions? dimensions = null,
		Duration? duration = null,
		bool isDeleted = false)
	{
		Kind = kind;
		Provider = provider;
		RemoteItemId = remoteItemId;
		DriveId = driveId;

		Name = name;
		Path = path;

		Size = size;
		MimeType = mimeType;
		Timestamps = timestamps;

		Hash = hash;
		Dimensions = dimensions;
		Duration = duration;

		IsDeleted = isDeleted;
	}

	/// <summary>
	/// Копирует базовые свойства из другого MediaItem (для маппинга из БД).
	/// </summary>
	public void ApplyFrom(MediaItem other)
	{
		Kind = other.Kind;
		Provider = other.Provider;
		RemoteItemId = other.RemoteItemId;
		DriveId = other.DriveId;
		Name = other.Name;
		Path = other.Path;
		Size = other.Size;
		MimeType = other.MimeType;
		Timestamps = other.Timestamps;
		Hash = other.Hash;
		Dimensions = other.Dimensions;
		Duration = other.Duration;
		IsDeleted = other.IsDeleted;
	}

	/// <summary>
	/// Императивный апдейт снапшотом синка (под bulk upsert).
	/// </summary>
	public void ApplySyncSnapshot(
        MediaKind kind,
        string name,
        MediaPath path,
        FileSize size,
        MimeType mimeType,
        MediaTimestamps timestamps,
        ContentHash? hash,
        Dimensions? dimensions,
        Duration? duration,
        bool isDeleted)
    {
        Kind = kind;
        Name = name;
        Path = path;
        Size = size;
        MimeType = mimeType;
        Timestamps = timestamps;
        Hash = hash;
        Dimensions = dimensions;
        Duration = duration;
        IsDeleted = isDeleted;
    }
}
