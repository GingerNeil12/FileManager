using FileManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.Configurations;

internal class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
{
    public void Configure(EntityTypeBuilder<FileMetadata> builder)
    {
        builder
            .ToTable("FileMetadata")
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .HasColumnName("FileMetadataId");

        builder
            .Property(x => x.Location)
            .HasMaxLength(512)
            .IsRequired();

        builder
            .Property(x => x.OriginalName)
            .HasMaxLength(512)
            .IsRequired();

        builder
            .Property(x => x.Size)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .IsRequired(false);
        
        builder
            .Property(x => x.IsEncrypted)
            .IsRequired();

        builder
            .Property(x => x.UploadedOn)
            .IsRequired();

        builder
            .Property(x => x.RemovedOn)
            .IsRequired(false);

        builder
            .HasOne(x => x.UploadedBy)
            .WithMany(x => x.Uploads)
            .HasForeignKey(x => x.UploadedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Assignees)
            .WithOne(x => x.FileMetadata)
            .HasForeignKey(x => x.FileMetadataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}