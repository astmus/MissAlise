using Microsoft.EntityFrameworkCore;
using MissAlise.Entities.OneDrive;
using FsFile = MissAlise.Entities.OneDrive.FsFile;

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

	public virtual DbSet<Audio> Audios { get; set; }

	public virtual DbSet<FsFile> Files { get; set; }

	public virtual DbSet<Folder> Folders { get; set; }

	public virtual DbSet<ItemInfo> Items { get; set; }

	public virtual DbSet<Photo> Photos { get; set; }

	public virtual DbSet<Video> Videos { get; set; }

	public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ItemInfo>().UseTptMappingStrategy();
		modelBuilder.Entity<ItemInfo>(item =>
		{
			item.HasKey(p => p.Id).HasName("Itemid");
			item.Property(p => p.MimeType).HasColumnName("Mimetype");
			item.Property(p => p.CreatedDateTime).HasColumnName("Createddatetime");
			item.Property(p => p.ModifieDateTime).HasColumnName("Modifiedatetime");
		});

		modelBuilder.Entity<FsFile>().UseTptMappingStrategy();
		//modelBuilder.Entity<File>(file =>
		//{
		//	file.HasKey(p => p.Id).HasName("Itemid");			
		//});

		modelBuilder.Entity<Folder>(folder =>
		{
			folder.HasOne(c => c.Parent)
			  .WithMany(c => c.Folders)
			  .HasForeignKey(c => c.Parentfolderid)
			  .OnDelete(DeleteBehavior.Cascade);
			folder.HasMany(file => file.Files).WithOne(f => f.Folder);
		});

		modelBuilder.Entity<Audio>(audio =>
		{
			audio.Property(p => p.TrackCount).HasColumnName("Trackcount");
		});


		modelBuilder.Entity<Photo>();

		modelBuilder.Entity<Video>();

		modelBuilder.Entity<User>(user => user.HasKey(nameof(User.Id)));
	}

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//	=> optionsBuilder.UseNpgsql("Host=192.168.0.3;Database=missdb;Username=postgres;Password=postgres");
	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//=> optionsBuilder.UseNpgsql("Host=192.168.0.3;Database=files;Username=docker;Password=docker");
}
