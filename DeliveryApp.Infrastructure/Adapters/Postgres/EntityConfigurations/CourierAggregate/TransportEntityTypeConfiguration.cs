using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate
{
    public class TransportEntityTypeConfiguration : IEntityTypeConfiguration<Transport>
    {
        public void Configure(EntityTypeBuilder<Transport> builder)
        {
            builder.ToTable("transports");

            builder.HasKey(transport => transport.Id);
            builder.Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnName("id")
                .IsRequired();

            builder.Property(entity => entity.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.OwnsOne(entity => entity.Speed,
                a =>
                    {
                        a.Property(c => c.Value).HasColumnName("speed")
                            .IsRequired();
                        a.WithOwner();
                    });

            builder.Navigation(entity => entity.Speed).IsRequired();
        }
    }
}
