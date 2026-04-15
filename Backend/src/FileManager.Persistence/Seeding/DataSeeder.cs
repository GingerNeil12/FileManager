using FileManager.Domain.Common.Enums;
using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence.Seeding;

internal static class DataSeeder
{
    private const string SEED_USER_EXTERNAL_PROVIDER_ID = "auth0|69d27b6f4d3c46f81e8d2b97";
    private const string SEED_USER_EMAIL = "neil.earlam123@icloud.com";
    private const string SEED_USER_GIVEN_NAME = "Neil";
    private const string SEED_USER_FAMILY_NAME = "Earlam";

    internal static async Task SeedAsync(DbContext context, bool _, CancellationToken cancellationToken)
    {
        await SeedAdminUserAsync((ApplicationDbContext)context, cancellationToken);
        await SeedDepartmentsAsync((ApplicationDbContext)context, cancellationToken);
    }

    private static async Task SeedAdminUserAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        bool adminExists = await context.Users
            .AnyAsync(u => u.ExternalProviderId == SEED_USER_EXTERNAL_PROVIDER_ID, cancellationToken);

        if (adminExists)
        {
            return;
        }

        var admin = ApplicationUser.Create(
            SEED_USER_EXTERNAL_PROVIDER_ID,
            SEED_USER_EMAIL,
            SEED_USER_GIVEN_NAME,
            SEED_USER_FAMILY_NAME,
            UserRoles.InternalAdmin,
            departmentId: null
        );

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        bool hasDepartments = await context.Departments.AnyAsync();

        if (hasDepartments)
        {
            return;
        }

        var departments = new List<Department>
        {
            new() { Name = "HR"},
            new() { Name = "Engineering"},
            new() { Name = "Support"},
            new() { Name = "Sales"},
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync(cancellationToken);
    }
}
