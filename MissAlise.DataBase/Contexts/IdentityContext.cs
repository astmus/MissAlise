using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MissAlise.Application.Common;
using MissAlise.Entities.OneDrive;

namespace MissAlise.DataBase.Contexts
{
	public class IdentityContext : IdentityDbContext<AppUser>
	{
		public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
		{
		}
		protected IdentityContext()
		{
		}

		public DbSet<User> PendingUsers { get; set; }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//	=> optionsBuilder.UseSqlite("Data Source=..\\MissAlise.WebApi\\Identity.db");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<User>().ToTable("PendingUsers");
			modelBuilder.Entity<AccessInformation>().HasKey(c => c.IdToken);
			modelBuilder.Entity<AppUser>().HasOne(e => e.AccessData).WithOne().HasForeignKey<AccessInformation>("UserId").IsRequired(false).OnDelete(DeleteBehavior.Restrict);

			//.HasForeignKey<UserCredentials>(c => c.AppUserId);
		}
	}
}
