using PaymentsMatchingTool.Api.Contracts;
using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Tests.Contracts;

public class MatchBatchDtoTests
{
    private static MatchBatch NewBatch() => new()
    {
        Id = Guid.NewGuid(),
        CreatedAtUtc = new DateTime(2026, 7, 27, 10, 0, 0, DateTimeKind.Utc),
        SystemFileName = "system.csv",
        ProviderFileName = "provider.csv",
        TotalCount = 4,
        MatchedCount = 2,
        OnlySystemCount = 1,
        OnlyProviderCount = 1,
        AmountMismatchCount = 0,
    };

    [Fact]
    public void MatchBatchSummaryDto_From_MapsBatchMetadataAndSummary()
    {
        var batch = NewBatch();

        var dto = MatchBatchSummaryDto.From(batch);

        Assert.Equal(batch.Id, dto.Id);
        Assert.Equal(batch.CreatedAtUtc, dto.CreatedAtUtc);
        Assert.Equal(batch.SystemFileName, dto.SystemFileName);
        Assert.Equal(batch.ProviderFileName, dto.ProviderFileName);
        Assert.Equal(4, dto.Summary.Total);
        Assert.Equal(2, dto.Summary.Matched);
        Assert.Equal(1, dto.Summary.OnlySystem);
        Assert.Equal(1, dto.Summary.OnlyProvider);
        Assert.Equal(0, dto.Summary.AmountMismatch);
    }

    [Fact]
    public void MatchBatchDetailResponse_From_MapsBatchMetadataSummaryAndItems()
    {
        var batch = NewBatch();
        var items = new List<PaymentMatch>
        {
            new()
            {
                Id = Guid.NewGuid(),
                BatchId = batch.Id,
                OrderId = "ORD-1",
                Currency = "USD",
                SystemAmount = 100m,
                ProviderAmount = 100m,
                Status = MatchStatus.Matched,
                Resolved = false,
            },
        };

        var dto = MatchBatchDetailResponse.From(batch, items);

        Assert.Equal(batch.Id, dto.BatchId);
        Assert.Equal(batch.CreatedAtUtc, dto.CreatedAtUtc);
        Assert.Equal(batch.SystemFileName, dto.SystemFileName);
        Assert.Equal(batch.ProviderFileName, dto.ProviderFileName);
        Assert.Equal(batch.TotalCount, dto.Summary.Total);

        var item = Assert.Single(dto.Items);
        Assert.Equal("ORD-1", item.OrderId);
        Assert.Equal("USD", item.Currency);
        Assert.Equal(MatchStatus.Matched.ToString(), item.Status);
    }
}
