using Microsoft.EntityFrameworkCore;
using MissAlise.DataBase.Models;

namespace MissAlise.DataBase.Contexts;

public partial class UserMediaContext : DbContext
{
	public UserMediaContext()
	{
	}

	public UserMediaContext(DbContextOptions<UserMediaContext> options)
		: base(options)
	{
	}

	public DbSet<DbMediaItem> MediaItems => Set<DbMediaItem>();
	public DbSet<DbPhoto> Photos => Set<DbPhoto>();
	public DbSet<DbVideo> Videos => Set<DbVideo>();
	public DbSet<DbAudio> Audios => Set<DbAudio>();
	public DbSet<DbMediaLibrary> MediaLibraries => Set<DbMediaLibrary>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<DbMediaItem>(b =>
		{
			b.ToTable("MediaItems");

			b.Property(x => x.Kind).HasConversion<byte>().HasColumnType("smallint").IsRequired();
			b.Property(x => x.Provider).HasConversion<byte>().HasColumnType("smallint").IsRequired();
			b.Property(x => x.RemoteItemId).IsRequired();
			b.Property(x => x.Name).IsRequired();
			b.Property(x => x.MimeType).IsRequired();

			// Helpful for deltas / updates:
			b.HasIndex(x => new { x.Provider, x.RemoteItemId, x.DriveId, x.OwnerId }).IsUnique();
			b.HasIndex(x => new { x.Kind, x.IsDeleted, x.TakenAt });
			b.HasIndex(x => new { x.Kind, x.IsDeleted, x.ModifiedAt });
		});

		modelBuilder.Entity<DbMediaLibrary>(b =>
		{
			b.ToTable("MediaLibraries");

			b.Property(x => x.OwnerId).IsRequired();
			b.Property(x => x.Provider).IsRequired();
			b.Property(x => x.CreatedAt).IsRequired();
			b.Property(x => x.UpdatedAt).IsRequired();

			b.HasIndex(x => new { x.OwnerId, x.Provider }).IsUnique();
		});

		modelBuilder.Entity<DbMediaItem>().UseTptMappingStrategy();
		modelBuilder.Entity<DbPhoto>().ToTable("Photos");
		modelBuilder.Entity<DbVideo>().ToTable("Videos");
		modelBuilder.Entity<DbAudio>().ToTable("Audios");
	}
}
