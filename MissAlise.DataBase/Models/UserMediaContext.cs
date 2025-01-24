using Microsoft.EntityFrameworkCore;
using MissAlise.Entities.OneDrive;
using File = MissAlise.Entities.OneDrive.File;

namespace MissAlise.DataBase.Models;

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

	public virtual DbSet<File> Files { get; set; }

	public virtual DbSet<Folder> Folders { get; set; }

	public virtual DbSet<Item> Items { get; set; }

	public virtual DbSet<Photo> Photos { get; set; }

	public virtual DbSet<Video> Videos { get; set; }

	public virtual DbSet<User> Users { get; set; }

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//	=> optionsBuilder.UseNpgsql("Host=localhost;Database=missdb;Username=docker;Password=docker");
	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//=> optionsBuilder.UseNpgsql("Host=192.168.0.3;Database=files;Username=docker;Password=docker");
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		
		modelBuilder.Entity<Item>().UseTptMappingStrategy();
		modelBuilder.Entity<File>().UseTptMappingStrategy();

		modelBuilder.Entity<Folder>(
			folder => {
				folder.HasOne(c => c.Parent)
				  .WithMany(c => c.Folders)
				  .HasForeignKey(c => c.Parentfolderid)
				  .OnDelete(DeleteBehavior.Restrict);

				folder.HasMany(file => file.Files).WithOne(f => f.Folder);
			  }
			  );

		modelBuilder.Entity<Audio>();

		modelBuilder.Entity<Photo>();

		modelBuilder.Entity<Video>();

		modelBuilder.Entity<User>(user
			=> 
			{ 
				user.HasKey(nameof(User.Id));			
			});
	}
}
