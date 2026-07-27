using Microsoft.EntityFrameworkCore;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MatchBatch> MatchBatches => Set<MatchBatch>();
    public DbSet<PaymentMatch> PaymentMatches => Set<PaymentMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
