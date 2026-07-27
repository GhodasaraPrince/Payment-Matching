using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Infrastructure.Configurations;

public class MatchBatchConfiguration : IEntityTypeConfiguration<MatchBatch>
{
    public void Configure(EntityTypeBuilder<MatchBatch> builder)
    {
        builder.ToTable("MatchBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.SystemFileName).HasMaxLength(260).IsRequired();
        builder.Property(b => b.ProviderFileName).HasMaxLength(260).IsRequired();

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Batch)
            .HasForeignKey(i => i.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
