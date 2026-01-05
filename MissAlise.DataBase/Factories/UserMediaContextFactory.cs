using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MissAlise.DataBase.Contexts;

namespace MissAlise.DataBase.Factories;

/*
 dotnet ef migrations add <migration name> \
  --project MissAlise.DataBase \
  --context UserMediaContext
  --output-dir Migrations/UserMedia
 */

public sealed class UserMediaContextFactory : IDesignTimeDbContextFactory<UserMediaContext>
{
    public UserMediaContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<UserMediaContext>()
            .UseNpgsql(
                "Host=192.168.0.3;Database=missdb;Username=postgres;Password=postgres")
            .Options;

        return new UserMediaContext(options);
    }
}
