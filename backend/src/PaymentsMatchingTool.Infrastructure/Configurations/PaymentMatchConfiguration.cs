using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Infrastructure.Configurations;

public class PaymentMatchConfiguration : IEntityTypeConfiguration<PaymentMatch>
{
    public void Configure(EntityTypeBuilder<PaymentMatch> builder)
    {
        builder.ToTable("PaymentMatches");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.OrderId).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Currency).HasMaxLength(3).IsRequired();
        builder.Property(i => i.SystemAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.ProviderAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.ResolutionSide).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => new { i.BatchId, i.OrderId, i.Currency }).IsUnique();
    }
}
