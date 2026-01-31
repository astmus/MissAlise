namespace MissAlise.OneDrive.Mapping;

using MissAlise.Entities.Media;
using MissAlise.OneDrive.Models;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public static class DriveItemMapper
{
	public static MediaItem ToMediaItem(this DriveItem item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		return new MediaItem(
			kind: GetKind(item),
			provider: ProviderKind.OneDrive,
			remoteItemId: item.Id ?? "",
			driveId: item.ParentReference?.DriveId,

			name: item.Name ?? "",
			path: new MediaPath(item.ParentReference?.Path ?? "/"), // адаптировать путь

			size: new FileSize(item.Size ?? 0),
			mimeType: new MimeType(item.File?.MimeType ?? "application/octet-stream"),
			hash: TryGetHash(item),

			timestamps: new MediaTimestamps(item.CreatedDateTime!.Value, item.LastModifiedDateTime!.Value),

			dimensions: GetDimensions(item),
			duration: GetDuration(item),

			isDeleted: item.Deleted != null
		)
		{
			Id = new MediaItemId(item.Id), // предполагается, что MediaItemId(string)

		};
	}

	private static MediaKind GetKind(DriveItem item)
	{
		var mime = item.File.MimeType?.ToLowerInvariant();

		if (mime?.StartsWith("image/") == true) return MediaKind.Photo;
		if (mime?.StartsWith("video/") == true) return MediaKind.Video;
		if (mime?.StartsWith("audio/") == true) return MediaKind.Audio;

		throw new ArgumentException($"mime type is not valid {item.File.MimeType}");
	}

	private static ContentHash? TryGetHash(DriveItem item)
	{
		var hash = item.File?.Hashes?.Sha256Hash;
		return string.IsNullOrWhiteSpace(hash) ? null : new ContentHash("Sha256Hash", hash);
	}

	private static Dimensions? GetDimensions(DriveItem item)
	{
		if (item.Image != null)
		{
			return new Dimensions(item.Image.Width ?? 0, item.Image.Height ?? 0);
		}

		if (item.Video != null)
		{
			return new Dimensions(item.Video.Width ?? 0, item.Video.Height ?? 0);
		}

		return null;
	}

	private static Duration? GetDuration(DriveItem item)
	{
		if (item.Video != null && item.Video.Duration != null)
		{
			return new Duration(TimeSpan.FromMilliseconds((double)item.Video.Duration));
		}

		if (item.Audio != null && item.Audio.Duration != null)
		{
			return new Duration(TimeSpan.FromMilliseconds((double)item.Audio.Duration));
		}

		return null;
	}
}