using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MissAlise.DataBase.Contexts;

/*
 dotnet ef migrations add <migration name> \
  --project MissAlise.DataBase \
  --context IdentityContext
  --output-dir Migrations/Identity
 */

namespace MissAlise.DataBase.Factories
{
	public sealed class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
	{
		public IdentityContext CreateDbContext(string[] args)
		{
			var options = new DbContextOptionsBuilder<IdentityContext>()
				.UseNpgsql(
					"Host=192.168.0.3;Database=miss_identity;Username=postgres;Password=postgres")
				.Options;

			return new IdentityContext(options);
		}
	}
}
