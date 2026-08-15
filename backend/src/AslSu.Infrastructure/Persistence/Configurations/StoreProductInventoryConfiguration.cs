using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class StoreProductInventoryConfiguration : IEntityTypeConfiguration<StoreProductInventory>
{
    public void Configure(EntityTypeBuilder<StoreProductInventory> builder)
    {
        builder.ToTable("StoreProductInventories");
        builder.HasKey(i => i.Id);

        builder.HasIndex(i => new { i.ProductId, i.StoreId }).IsUnique();

        builder.Property(i => i.SalePrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.ListPrice).HasColumnType("decimal(10,2)");
        builder.Property(i => i.LastSyncedSalePrice).HasColumnType("decimal(10,2)");

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(i => i.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
