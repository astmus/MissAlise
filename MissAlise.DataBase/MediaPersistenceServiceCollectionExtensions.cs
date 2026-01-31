namespace MissAlise.DataBase;

using Microsoft.Extensions.DependencyInjection;
using MissAlise.DataBase.Repositories;
using MissAlise.Interfaces;

public static class MediaPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers Postgres(EF Core / Npgsql) repositories for the Media domain.
    /// Assumes UserMediaContext is already registered as DbContext.
    /// </summary>
    public static IServiceCollection AddMediaPersistence(this IServiceCollection services)
    {
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IMediaLibraryRepository, MediaLibraryRepository>();
        return services;
    }
}
