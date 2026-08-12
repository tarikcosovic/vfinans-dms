using DMS.Domain.Entities;
using DMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.OwnerUserId).HasColumnName("owner_user_id").IsRequired();

        builder.Property(d => d.FileKey).HasColumnName("file_key").IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion(s => s.ToString(), s => Enum.Parse<DocumentStatus>(s))
            .IsRequired();

        builder.Property(d => d.FileName).HasColumnName("file_name").IsRequired();
        builder.Property(d => d.Rename).HasColumnName("rename").HasMaxLength(255).IsRequired();
        builder.Property(d => d.ContentType).HasColumnName("content_type").IsRequired();
        builder.Property(d => d.DocumentType)
            .HasColumnName("document_type")
            .HasConversion(s => s.ToString(), s => Enum.Parse<DocumentType>(s))
            .IsRequired();
        builder.Property(d => d.SizeBytes).HasColumnName("size_bytes");
        builder.Property(d => d.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(d => d.Notes).HasColumnName("notes");
        builder.Property(d => d.IsDownloaded).HasColumnName("downloaded").HasDefaultValue(false);
        builder.Property(d => d.IsRead).HasColumnName("is_read").HasDefaultValue(false);

        builder.HasIndex(d => d.OwnerUserId).HasDatabaseName("ix_documents_owner_user_id");
        builder.HasIndex(d => d.Status).HasDatabaseName("ix_documents_status");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
