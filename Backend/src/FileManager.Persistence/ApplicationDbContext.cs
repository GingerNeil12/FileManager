using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;

namespace FileManager.Persistence;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<FileMetadata> FileMetadata => Set<FileMetadata>();
    public DbSet<FileMember> FileMembers => Set<FileMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}