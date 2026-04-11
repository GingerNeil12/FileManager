using FileManager.Application.Clients;
using FileManager.Application.Common.Interfaces;
using FileManager.Application.Options;
using FileManager.Application.Workflows.CreateUser;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.Configure<Auth0ManagementOptions>(
            configuration.GetSection(Auth0ManagementOptions.SectionName));

        services.AddMemoryCache();
        services.AddHttpClient<IAuth0ManagementClient, Auth0ManagementClient>();

        services
            .AddScoped<ICreateUserService, CreateUserService>();

        return services;
    }
}
