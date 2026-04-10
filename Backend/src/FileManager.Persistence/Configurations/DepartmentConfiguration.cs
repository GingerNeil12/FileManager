using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder
            .ToTable("Department")
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("DepartmentId");

        builder
            .Property(x => x.Name)
            .HasMaxLength(512)
            .IsRequired();

        builder
            .HasMany(x => x.Members)
            .WithOne(x => x.Department)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}