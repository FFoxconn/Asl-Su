using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.PackageId).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.PackageId).IsUnique();

        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(o => o.WorkflowStatus).HasConversion<string>().HasMaxLength(50);

        builder.Property(o => o.InvoiceAmount).HasColumnType("decimal(10,2)");
        builder.Property(o => o.InvoiceTaxAmount).HasColumnType("decimal(10,2)");
        builder.Property(o => o.ReceiptLink).HasMaxLength(1000);

        builder.HasOne<Store>()
            .WithMany()
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Courier>()
            .WithMany()
            .HasForeignKey(o => o.CourierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
