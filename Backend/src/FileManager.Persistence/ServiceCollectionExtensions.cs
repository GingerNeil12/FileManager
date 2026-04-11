using Azure.Storage.Blobs;

using FileManager.Application.Common.Interfaces;
using FileManager.Persistence.Repositories;
using FileManager.Persistence.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer(configuration.GetConnectionString("SqlServer"))
                    .UseAsyncSeeding(DataSeeder.SeedAsync)
                    .UseSeeding((_, _) => { }));


        services.AddSingleton<BlobContainerClient>(_ =>
        {
            var containerClient = new BlobContainerClient(
                configuration.GetConnectionString("BlobStorage"),
                configuration["Blob:ContainerName"]
            );

            containerClient.CreateIfNotExists();

            return containerClient;
        });

        services
            .AddScoped<IApplicationUserRepository, ApplicationUserRepository>()
            .AddScoped<IFileMetadataRepository, FileMetadataRepository>()
            .AddScoped<IBlobRepository, BlobRepository>();

        return services;
    }
}