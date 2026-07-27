using Microsoft.EntityFrameworkCore;
using PaymentsMatchingTool.Application.Csv;
using PaymentsMatchingTool.Application.Matching;
using PaymentsMatchingTool.Domain;
using PaymentsMatchingTool.Infrastructure.Services;

namespace PaymentsMatchingTool.Infrastructure.Tests.Services;

public class MatchRunServiceTests
{
    private static MatchRunService CreateService(AppDbContext db) =>
        new(db, new CsvPaymentParser(), new PaymentMatchingService());

    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static MatchBatch NewBatch(DateTime createdAtUtc, string systemFileName = "system.csv", string providerFileName = "provider.csv") => new()
    {
        Id = Guid.NewGuid(),
        CreatedAtUtc = createdAtUtc,
        SystemFileName = systemFileName,
        ProviderFileName = providerFileName,
        TotalCount = 0,
        MatchedCount = 0,
        OnlySystemCount = 0,
        OnlyProviderCount = 0,
        AmountMismatchCount = 0,
    };

    [Fact]
    public async Task GetAllBatchesAsync_NoBatches_ReturnsEmptyList()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var (result, totalCount) = await service.GetAllBatchesAsync();

        Assert.Empty(result);
        Assert.Equal(0, totalCount);
    }

    [Fact]
    public async Task GetAllBatchesAsync_MultipleBatches_OrdersByCreatedAtUtcDescending()
    {
        await using var db = CreateDb();
        var oldest = NewBatch(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var middle = NewBatch(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        var newest = NewBatch(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc));
        db.MatchBatches.AddRange(oldest, middle, newest);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var (result, totalCount) = await service.GetAllBatchesAsync();

        Assert.Equal(new[] { newest.Id, middle.Id, oldest.Id }, result.Select(b => b.Id));
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task GetAllBatchesAsync_PageSizeSmallerThanTotal_ReturnsOnlyThatPage()
    {
        await using var db = CreateDb();
        var oldest = NewBatch(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var middle = NewBatch(new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        var newest = NewBatch(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc));
        db.MatchBatches.AddRange(oldest, middle, newest);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var (result, totalCount) = await service.GetAllBatchesAsync(page: 1, pageSize: 2);

        Assert.Equal(new[] { newest.Id, middle.Id }, result.Select(b => b.Id));
        Assert.Equal(3, totalCount);
    }
}
