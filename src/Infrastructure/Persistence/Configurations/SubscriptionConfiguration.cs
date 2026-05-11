using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.SubscriptionType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.ServiceProviderName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.SubscriptionNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CurrentDebtAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.NextDueDate)
            .IsRequired();

        builder.Property(s => s.CreatedDate)
            .IsRequired();

        builder.Property(s => s.UpdatedDate)
            .IsRequired();

        // Relationship: Subscription -> Payments (1:N) with Restrict delete
        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Subscription)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
