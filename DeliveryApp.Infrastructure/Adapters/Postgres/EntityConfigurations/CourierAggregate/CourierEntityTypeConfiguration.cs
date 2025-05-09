using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate
{
    public class CourierEntityTypeConfiguration : IEntityTypeConfiguration<Courier>
    {
        public void Configure(EntityTypeBuilder<Courier> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("couriers");

            entityTypeBuilder.HasKey(courier => courier.Id);
            entityTypeBuilder.Property(entity => entity.Id)
                .ValueGeneratedNever()
                .HasColumnName("id")
                .IsRequired();

            entityTypeBuilder.Property(entity => entity.Name)
                .HasColumnName("name")
                .IsRequired();

            entityTypeBuilder.OwnsOne(entity => entity.Status,
                a =>
                    {
                        a.Property(c => c.Name).HasColumnName("status")
                            .IsRequired();
                        a.WithOwner();
                    });

            entityTypeBuilder.Navigation(entity => entity.Status).IsRequired();

            entityTypeBuilder.OwnsOne(
                entity => entity.Location,
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

            entityTypeBuilder
                .HasOne(entity => entity.Transport)
                .WithMany()
                .IsRequired()
                .HasForeignKey("transport_id");
        }
    }
}
