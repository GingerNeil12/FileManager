using FileManager.Domain.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileManager.Persistence.Configurations;

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .ToTable("ApplicationUser")
            .HasKey(x => x.Id);
        
        builder
            .Property(x => x.Id)
            .HasColumnName("UserId");
        
        builder
            .Property(x => x.ExternalProviderId)
            .HasMaxLength(128)
            .IsRequired();
        
        builder
            .HasIndex(x => x.ExternalProviderId)
            .IsUnique();

        builder
            .Property(x => x.CreatedOn)
            .IsRequired();
        
        builder
            .HasMany(x => x.Uploads)
            .WithOne(x => x.UploadedBy)
            .HasForeignKey(x => x.UploadedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.AssignedUploads)
            .WithOne(x => x.AssignedTo)
            .HasForeignKey(x => x.AssignedToId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}