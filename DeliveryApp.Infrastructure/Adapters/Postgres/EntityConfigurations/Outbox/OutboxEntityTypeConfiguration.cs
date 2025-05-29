using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox
{
    public class OutboxEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> entityTypeBuilder)
        {
            entityTypeBuilder
                .ToTable("outbox");

            entityTypeBuilder
                .Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entityTypeBuilder
                .Property(entity => entity.Type)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("type")
                .IsRequired();

            entityTypeBuilder
                .Property(entity => entity.Content)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("content")
                .IsRequired();

            entityTypeBuilder
                .Property(entity => entity.OccurredOnUtc)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("occurred_on_utc")
                .IsRequired();

            entityTypeBuilder
                .Property(entity => entity.ProcessedOnUtc)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("processed_on_utc")
                .IsRequired(false);
        }
    }
}
