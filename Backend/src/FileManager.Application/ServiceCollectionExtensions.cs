using FileManager.Application.Workflows.CreateUser;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        
        services
            .AddScoped<ICreateUserService, CreateUserService>();

        return services;
    }
}