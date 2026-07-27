using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record MatchSummaryDto(int Total, int Matched, int OnlySystem, int OnlyProvider, int AmountMismatch)
{
    public static MatchSummaryDto From(MatchBatch batch) => new(
        batch.TotalCount,
        batch.MatchedCount,
        batch.OnlySystemCount,
        batch.OnlyProviderCount,
        batch.AmountMismatchCount);
}
