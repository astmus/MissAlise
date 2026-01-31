namespace MissAlise.DataBase.Models;

using System.ComponentModel.DataAnnotations;
using MissAlise.ValueObjects.Media;
using MissAlise.ValueObjects.Storage;

public sealed class DbMediaLibrary
{
    [Key]
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    // ProviderKind (int)
    public ProviderKind Provider { get; set; }

    // Saved delta link/token from Graph delta API (opaque string).
    public string? DeltaToken { get; set; }

	public DateTimeOffset? LastSyncStartedAt { get; set; }
	public DateTimeOffset? LastSyncFinishedAt { get; set; }
	public SyncStatus LastSyncStatus { get; set; } 
	public string? LastSyncError { get; set; }

	public DateTimeOffset? LastFullSyncAt { get; set; }
    public DateTimeOffset? LastDeltaSyncAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
