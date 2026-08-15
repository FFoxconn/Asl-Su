using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.TgoStoreId).HasMaxLength(100);
    }
}
