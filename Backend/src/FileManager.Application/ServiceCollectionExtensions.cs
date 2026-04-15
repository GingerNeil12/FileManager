using FileManager.Application.Clients;
using FileManager.Application.Common.Interfaces;
using FileManager.Application.Options;
using FileManager.Application.Workflows.CreateUser;
using FileManager.Application.Workflows.GetFilteredDepartments;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FileManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.Configure<Auth0ManagementOptions>(
            configuration.GetSection(Auth0ManagementOptions.SECTION_NAME));

        services.AddMemoryCache();

        services.AddHttpClient("auth0-token", (sp, client) =>
        {
            Auth0ManagementOptions opts = sp.GetRequiredService<IOptions<Auth0ManagementOptions>>().Value;
            client.BaseAddress = new Uri($"{opts.Scheme}://{opts.Domain}");
        });

        services.AddTransient<Auth0TokenHandler>();

        services.AddHttpClient<IAuth0ManagementClient, Auth0ManagementClient>((sp, client) =>
        {
            Auth0ManagementOptions opts = sp.GetRequiredService<IOptions<Auth0ManagementOptions>>().Value;
            client.BaseAddress = new Uri($"{opts.Scheme}://{opts.Domain}");
        })
        .AddHttpMessageHandler<Auth0TokenHandler>();

        services
            .AddScoped<ICreateUserService, CreateUserService>()
            .AddScoped<IGetFilteredDepartmentsService, GetFilteredDepartmentsService>();

        return services;
    }
}
