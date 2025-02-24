using Microsoft.EntityFrameworkCore;
using MissAlise.Entities.OneDrive;

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
	public virtual DbSet<DataFile> Files { get; set; }
	public virtual DbSet<Folder> Folders { get; set; }
	public virtual DbSet<Photo> Photos { get; set; }
	public virtual DbSet<Video> Videos { get; set; }
	public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ItemInfo>().UseTptMappingStrategy();
		//modelBuilder.Entity<ItemInfo>(item =>
		//{
		//	item.HasKey(p => p.Id).HasName("Id");
		//	item.Property(p => p.MimeType).HasColumnName("Mimetype");
		//	item.Property(p => p.CreatedDateTime).HasColumnName("Createddatetime");
		//	item.Property(p => p.ModifieDateTime).HasColumnName("Modifiedatetime");
		//});

		modelBuilder.Entity<DataFile>().UseTptMappingStrategy();
		//modelBuilder.Entity<File>(file =>
		//{
		//	file.HasKey(p => p.Id).HasName("Itemid");			
		//});
		modelBuilder.Entity<Folder>();
		//modelBuilder.Entity<Folder>(folder =>
		//{
		//	folder.HasOne(c => c.Parent)
		//	  .WithMany(c => c.Folders)
		//	  .HasForeignKey(c => c.Parentfolderid)
		//	  .OnDelete(DeleteBehavior.Cascade);
		//	folder.HasMany(file => file.Files).WithOne(f => f.Folder);
		//});

		modelBuilder.Entity<Audio>();
		modelBuilder.Entity<Photo>();
		modelBuilder.Entity<Video>();
		modelBuilder.Entity<User>();
	}

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//	=> optionsBuilder.UseNpgsql("Host=192.168.0.3;Database=missdb;Username=postgres;Password=postgres");
	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//	=> optionsBuilder.UseNpgsql("Host=localhost;Database=missdb;Username=postgres;Password=postgres");
}
