namespace MissAlise.DataBase.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public class DbMediaItem
{
    [Key]
    public required string Id { get; set; }

    // Domain: MediaKind (int)
    public MediaKind Kind { get; set; }

    // Domain: ProviderKind (int)
    public ProviderKind Provider { get; set; }

	public Guid OwnerId { get; set; }

    [MaxLength(512)]
    public string RemoteItemId { get; set; } = null!;

    [MaxLength(256)]
    public string? DriveId { get; set; }

    [MaxLength(1024)]
    public string Name { get; set; } = null!;

    [MaxLength(2048)]
    public string? ParentPath { get; set; }

    [MaxLength(256)]
    public string MimeType { get; set; } = null!;

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public DateTimeOffset? TakenAt { get; set; }

    public bool IsDeleted { get; set; }

    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? DurationSeconds { get; set; }

    [MaxLength(64)]
    public string? HashAlgorithm { get; set; }

    [MaxLength(512)]
    public string? HashValue { get; set; }
}
