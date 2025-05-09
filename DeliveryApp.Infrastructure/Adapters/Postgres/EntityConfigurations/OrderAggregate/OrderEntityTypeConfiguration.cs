using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate
{
    public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("orders");

            entityTypeBuilder.HasKey(order => order.Id);
            entityTypeBuilder.Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnName("id")
                .IsRequired();

            entityTypeBuilder.Property(entity => entity.CourierId)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("courier_id")
                .IsRequired(false); 

            entityTypeBuilder.OwnsOne(
                entity => entity.Status,
                a =>
                    {
                        a.Property(c => c.Name)
                            .HasColumnName("status")
                            .IsRequired();
                        a.WithOwner();
                    });

            entityTypeBuilder.Navigation(entity => entity.Status).IsRequired();

            entityTypeBuilder.OwnsOne(entity => entity.Location,
                a =>
                    {
                        a.Property(c => c.X)
                            .HasColumnName("location_x")
                            .IsRequired();
                        a.Property(c => c.Y)
                            .HasColumnName("location_y")
                            .IsRequired();

                        a.WithOwner();
                    });

            entityTypeBuilder.Navigation(entity => entity.Location).IsRequired();
        }
    }
}
