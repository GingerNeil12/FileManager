using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.Configurations;

public class FileDepartmentConfiguration : IEntityTypeConfiguration<FileDepartment>
{
    public void Configure(EntityTypeBuilder<FileDepartment> builder)
    {
        builder
            .ToTable("FileDepartment")
            .HasKey(x => new { x.FileMetadataId, x.DepartmentId });

        builder
            .Property(x => x.AssignedOn)
            .IsRequired();

        builder
            .Property(x => x.DownloadedOn)
            .IsRequired(false);

        builder
            .Property(x => x.DeletedOn)
            .IsRequired(false);

        builder
            .Property(x => x.DeletedBy)
            .IsRequired(false);
        
        builder
            .Property(x => x.DownloadedBy)
            .IsRequired(false);

        builder
            .HasOne(x => x.FileMetadata)
            .WithMany(x => x.Departments)
            .HasForeignKey(x => x.FileMetadataId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Department)
            .WithMany(x => x.AssignedUploads)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}