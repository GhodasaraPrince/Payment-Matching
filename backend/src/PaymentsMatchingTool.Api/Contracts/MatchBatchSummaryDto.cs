using PaymentsMatchingTool.Domain;

namespace PaymentsMatchingTool.Api.Contracts;

public record MatchBatchSummaryDto(
    Guid Id,
    DateTime CreatedAtUtc,
    string SystemFileName,
    string ProviderFileName,
    MatchSummaryDto Summary)
{
    public static MatchBatchSummaryDto From(MatchBatch batch) => new(
        batch.Id,
        batch.CreatedAtUtc,
        batch.SystemFileName,
        batch.ProviderFileName,
        MatchSummaryDto.From(batch));
}
