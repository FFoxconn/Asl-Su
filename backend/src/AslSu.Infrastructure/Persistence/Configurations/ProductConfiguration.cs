using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Barcode).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Barcode).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.VatRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.ImageUrl).HasMaxLength(1000);

        builder.Property(p => p.TgoProductId).HasMaxLength(100);
        builder.Property(p => p.TgoBarcode).HasMaxLength(100);
        builder.Property(p => p.TgoSyncStatus).HasConversion<string>().HasMaxLength(50);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Brand>()
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
