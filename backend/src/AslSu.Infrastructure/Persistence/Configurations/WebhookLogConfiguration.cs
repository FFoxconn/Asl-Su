using AslSu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AslSu.Infrastructure.Persistence.Configurations;

public class WebhookLogConfiguration : IEntityTypeConfiguration<WebhookLog>
{
    public void Configure(EntityTypeBuilder<WebhookLog> builder)
    {
        builder.ToTable("WebhookLogs");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.EventType).IsRequired().HasMaxLength(100);

        builder.Property(w => w.ExternalEventId).IsRequired().HasMaxLength(200);
        builder.HasIndex(w => w.ExternalEventId).IsUnique();

        builder.Property(w => w.RawPayloadJson).IsRequired();
        builder.Property(w => w.ProcessingStatus).HasConversion<string>().HasMaxLength(50);
    }
}
