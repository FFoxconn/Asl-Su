using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class SyncLogConfiguration : IEntityTypeConfiguration<SyncLog>
{
    public void Configure(EntityTypeBuilder<SyncLog> builder)
    {
        builder.ToTable("SyncLogs");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OperationType).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Endpoint).IsRequired().HasMaxLength(500);
        builder.Property(s => s.HttpMethod).IsRequired().HasMaxLength(10);
        builder.Property(s => s.SupplierId).IsRequired().HasMaxLength(100);
        builder.Property(s => s.BatchRequestId).HasMaxLength(100);
        builder.Property(s => s.OrderNumber).HasMaxLength(100);
        builder.HasIndex(s => s.Timestamp);
    }
}
