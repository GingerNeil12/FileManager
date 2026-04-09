using FileManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.Configurations;

internal class FileMemberConfiguration : IEntityTypeConfiguration<FileMember>
{
    public void Configure(EntityTypeBuilder<FileMember> builder)
    {
        builder
            .ToTable("FileMember")
            .HasKey(x => new { x.FileMetadataId, x.AssignedToId });

        builder
            .Property(x => x.AssignedOn)
            .IsRequired();

        builder
            .Property(x => x.DeletedOn)
            .IsRequired(false);

        builder
            .Property(x => x.DownloadedOn)
            .IsRequired(false);

        builder
            .HasOne(x => x.AssignedTo)
            .WithMany(x => x.AssignedUploads)
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.FileMetadata)
            .WithMany(x => x.Assignees)
            .HasForeignKey(x => x.FileMetadataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}