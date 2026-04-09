using Azure.Storage.Blobs;
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
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services
            .AddSingleton(async () =>
            {
                var containerClient = new BlobContainerClient(
                    configuration.GetConnectionString("BlobStorage"),
                    configuration["Blob:ContainerName"]
                );

                await containerClient.CreateIfNotExistsAsync();

                return containerClient;
            });

        return services;
    }
}