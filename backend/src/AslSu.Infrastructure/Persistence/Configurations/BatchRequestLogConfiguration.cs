using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class BatchRequestLogConfiguration : IEntityTypeConfiguration<BatchRequestLog>
{
    public void Configure(EntityTypeBuilder<BatchRequestLog> builder)
    {
        builder.ToTable("BatchRequestLogs");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchRequestId).IsRequired().HasMaxLength(100);
        builder.HasIndex(b => b.BatchRequestId);

        builder.Property(b => b.OperationType).HasConversion<string>().HasMaxLength(50);
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(50);
    }
}
